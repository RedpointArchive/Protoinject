using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.IO;

namespace Protoinject.FactoryGenerator
{
    using System.Collections.Generic;

    public static class Program
    {
        /// <remarks>
        /// We can't use Import because it finds the type in the current framework, not the
        /// framework that the assembly is referencing.
        /// </remarks>
        private static TypeDefinition FindTypeInModuleOrReferences(AssemblyDefinition assembly, string name, List<string> visited = null, AssemblyDefinition original = null)
        {
            if (visited == null)
            {
                visited = new List<string>();
            }

            if (original == null)
            {
                original = assembly;
            }

            visited.Add(assembly.FullName);

            foreach (var module in assembly.Modules)
            {
                TypeDefinition definedType = module.Types.FirstOrDefault(x => x.FullName == name);
                if (definedType != null)
                {
                    Console.WriteLine("Resolved type '" + name + "' as " + definedType.FullName + " (in " + assembly.FullName + ")");
                    return definedType;
                }
                
                foreach (var @ref in module.AssemblyReferences)
                {
                    if (visited.Contains(@ref.FullName)) 
                    {
                        continue;
                    }

                    var assemblyResolved = module.AssemblyResolver.Resolve(@ref);

                    try
                    {
                        return FindTypeInModuleOrReferences(assemblyResolved, name, visited, original);
                    }
                    catch
                    {
                    }
                }
            }

            throw new Exception("Unable to resolve '" + name + "'");
        }

        public static void Main(string[] args)
        {
            var resolver = new PreloadedAssemblyResolver();
            foreach (var reference in args[1].Split(';'))
            {
                Console.WriteLine("Loading referenced assembly from " + reference + "...");
                resolver.Load(reference);
            }

            var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters {ReadSymbols = true, AssemblyResolver = resolver});
            Console.WriteLine("Generating factories for " + assembly.FullName + "...");
            
            var iGenerateFactory = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.IGenerateFactory"));
            var iNode = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.INode"));
            var iKernelDef = FindTypeInModuleOrReferences(assembly, "Protoinject.IKernel");
            var iCurrentNodeDef = FindTypeInModuleOrReferences(assembly, "Protoinject.ICurrentNode");
            var @object = assembly.MainModule.TypeSystem.Object;
            var @string = assembly.MainModule.TypeSystem.String;
            var objectConstructor = assembly.MainModule.Import(assembly.MainModule.TypeSystem.Object.Resolve().GetConstructors().First());
            var getNodeForFactoryImplementation = 
                assembly.MainModule.Import(iCurrentNodeDef.GetMethods().First(x => x.Name == "GetNodeForFactoryImplementation"));
            var getTypeFromHandle = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "System.Type").GetMethods().First(x => x.Name == "GetTypeFromHandle"));
            var iConstructorArgument = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.IConstructorArgument"));
            var namedConstructorArgumentConstructor = assembly.MainModule.Import(
                FindTypeInModuleOrReferences(assembly, "Protoinject.NamedConstructorArgument")
                .GetConstructors().First());
            var kernelGet = assembly.MainModule.Import(iKernelDef.GetMethods().First(x => 
                x.Name == "Get" &&
                x.Parameters.Count == 5 &&
                x.Parameters[0].ParameterType.Name == "Type" &&
                x.Parameters[1].ParameterType.Name == "INode" &&
                x.Parameters[2].ParameterType.Name == "String" &&
                x.Parameters[3].ParameterType.Name == "String"));
            var generatedFactoryAttributeConstructor = assembly.MainModule.Import(
                FindTypeInModuleOrReferences(assembly, "Protoinject.GeneratedFactoryAttribute").GetConstructors()
                .First());

            var iCurrentNode = assembly.MainModule.Import(iCurrentNodeDef);
            var iKernel = assembly.MainModule.Import(iKernelDef);

            var modified = false;

            var types = from module in assembly.Modules
                from type in module.Types
                where type.IsInterface && type.Interfaces.Any(x => x.FullName == iGenerateFactory.FullName)
                select type;
            foreach (var type in types.ToList())
            {
                if (type.CustomAttributes.Any(x => x.Constructor == generatedFactoryAttributeConstructor))
                {
                    Console.WriteLine("Factory already generated for: " + type);
                    continue;
                }

                Console.WriteLine("Generating factory: " + type);
                
                var factory = new TypeDefinition(
                    "_GeneratedFactories",
                    "Generated" + type.Name.Substring(1),
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit);
                factory.BaseType = @object;

                foreach (var gp in type.GenericParameters)
                {
                    var ngp = new GenericParameter(gp.Name, factory);
                    ngp.Attributes = gp.Attributes;
                    foreach (TypeReference gpc in gp.Constraints)
                        ngp.Constraints.Add(gpc);
                    factory.GenericParameters.Add(ngp);
                }
                
                if (factory.GenericParameters.Count > 0)
                {
                    factory.Interfaces.Add(type.MakeGenericInstanceType(factory.GenericParameters.Cast<TypeReference>().ToArray()));
                }
                else
                {
                    factory.Interfaces.Add(type);
                }

                var currentField = new FieldDefinition("_current", FieldAttributes.Private | FieldAttributes.InitOnly, iNode);
                factory.Fields.Add(currentField);

                var kernelField = new FieldDefinition("_kernel", FieldAttributes.Private | FieldAttributes.InitOnly, iKernel);
                factory.Fields.Add(kernelField);

                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.CompilerControlled |
                        MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.RTSpecialName,
                    type.Module.Import(typeof(void)));
                ctor.Body.InitLocals = true;
                factory.Methods.Add(ctor);

                ctor.Parameters.Add(new ParameterDefinition("node", ParameterAttributes.None, iCurrentNode));
                ctor.Parameters.Add(new ParameterDefinition("kernel", ParameterAttributes.None, iKernel));

                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectConstructor));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, getNodeForFactoryImplementation));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, currentField));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, kernelField));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                type.Module.Types.Add(factory);
                
                foreach (var method in type.Methods)
                {
                    var impl = new MethodDefinition(method.Name,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual,
                        method.ReturnType);
                    factory.Methods.Add(impl);
                    
                    foreach (var gp in method.GenericParameters)
                    {
                        var ngp = new GenericParameter(gp.Name, impl);
                        ngp.Attributes = gp.Attributes;
                        foreach (TypeReference gpc in gp.Constraints)
                            ngp.Constraints.Add(gpc);
                        impl.GenericParameters.Add(ngp);
                    }

                    foreach (var p in method.Parameters)
                    {
                        impl.Parameters.Add(new ParameterDefinition(p.ParameterType) { Name = p.Name });
                    }

                    impl.Body.InitLocals = true;

                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, kernelField));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, impl.ReturnType));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getTypeFromHandle));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, currentField));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Newarr, iConstructorArgument));

                    var i = 0;
                    foreach (var p in method.Parameters)
                    {
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Dup));
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, p.Name));
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, p));
                        if (p.ParameterType.IsValueType || p.ParameterType.IsGenericParameter)
                        {
                            impl.Body.Instructions.Add(Instruction.Create(OpCodes.Box, p.ParameterType));
                        }
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, namedConstructorArgumentConstructor));
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
                        i++;
                    }

                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, kernelGet));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                }

                var attribute = new CustomAttribute(generatedFactoryAttributeConstructor);
                attribute.ConstructorArguments.Add(new CustomAttributeArgument(@string, factory.FullName));
                type.CustomAttributes.Add(attribute);

                modified = true;
            }

            if (modified)
            {
                Console.WriteLine("Saving assembly: " + args[0]);
                assembly.Write(args[0], new WriterParameters {WriteSymbols = true});
            }
        }
    }
}