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

            var readSymbols =
                File.Exists(Path.Combine(Path.GetDirectoryName(args[0]),
                    Path.GetFileNameWithoutExtension(args[0]) + ".pdb")) ||
                File.Exists(Path.Combine(Path.GetDirectoryName(args[0]),
                    Path.GetFileNameWithoutExtension(args[0]) + ".dll.mdb"));
            var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters {ReadSymbols = readSymbols, AssemblyResolver = resolver});
            Console.WriteLine("Generating factories for " + assembly.FullName + "...");
            
            TypeReference iGenerateFactory;
            try
            {
                iGenerateFactory =
                    assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.IGenerateFactory"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Unable to resolve IGenerateFactory; this usually means the assembly does not use or reference " +
                    "Protoinject at all.  Skipping....");
                return;
            }

            var iNode = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.INode"));
            var iKernelDef = FindTypeInModuleOrReferences(assembly, "Protoinject.IKernel");
            var iCurrentNodeDef = FindTypeInModuleOrReferences(assembly, "Protoinject.ICurrentNode");
            var @object = assembly.MainModule.TypeSystem.Object;
            var @string = assembly.MainModule.TypeSystem.String;
            var objectConstructor = assembly.MainModule.Import(assembly.MainModule.TypeSystem.Object.Resolve().GetConstructors().First());
            var getNodeForFactoryImplementation = 
                assembly.MainModule.Import(iCurrentNodeDef.GetMethods().First(x => x.Name == "GetNodeForFactoryImplementation"));
            var getTypeFromHandle = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "System.Type").GetMethods().First(x => x.Name == "GetTypeFromHandle"));
            var notSupportedExceptionCtor = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "System.NotSupportedException").GetConstructors().First(x => x.Parameters.Count == 0));
            var iConstructorArgument = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.IConstructorArgument"));
            var iInjectionAttribute = assembly.MainModule.Import(FindTypeInModuleOrReferences(assembly, "Protoinject.IInjectionAttribute"));
            var namedConstructorArgumentConstructor = assembly.MainModule.Import(
                FindTypeInModuleOrReferences(assembly, "Protoinject.NamedConstructorArgument")
                .GetConstructors().First());
            var kernelGet = assembly.MainModule.Import(iKernelDef.GetMethods().First(x => 
                x.Name == "Get" &&
                x.Parameters.Count == 7 &&
                x.Parameters[0].ParameterType.Name == "Type" &&
                x.Parameters[1].ParameterType.Name == "INode" &&
                x.Parameters[2].ParameterType.Name == "String" &&
                x.Parameters[3].ParameterType.Name == "String"));
            var kernelGetAsync = assembly.MainModule.Import(iKernelDef.GetMethods().First(x =>
                x.Name == "GetAsync" &&
                x.Parameters.Count == 6 &&
                x.Parameters[0].ParameterType.Name == "INode" &&
                x.Parameters[1].ParameterType.Name == "String" &&
                x.Parameters[2].ParameterType.Name == "String"));
            var generatedFactoryAttributeConstructor = assembly.MainModule.Import(
                FindTypeInModuleOrReferences(assembly, "Protoinject.GeneratedFactoryAttribute").GetConstructors()
                .First(x => x.Parameters.Count == 2));

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
                    Console.WriteLine("Factories already generated for: " + type);
                    continue;
                }

                Console.WriteLine("Generating factories: " + type);
                
                var supportedFactory = new TypeDefinition(
                    "_GeneratedFactories",
                    "Generated" + type.Name.Substring(1),
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit);
                supportedFactory.BaseType = @object;

                foreach (var gp in type.GenericParameters)
                {
                    var ngp = new GenericParameter(gp.Name, supportedFactory);
                    ngp.Attributes = gp.Attributes;
                    foreach (TypeReference gpc in gp.Constraints)
                        ngp.Constraints.Add(gpc);
                    supportedFactory.GenericParameters.Add(ngp);
                }
                
                if (supportedFactory.GenericParameters.Count > 0)
                {
                    supportedFactory.Interfaces.Add(type.MakeGenericInstanceType(supportedFactory.GenericParameters.Cast<TypeReference>().ToArray()));
                }
                else
                {
                    supportedFactory.Interfaces.Add(type);
                }

                var currentField = new FieldDefinition("_current", FieldAttributes.Private | FieldAttributes.InitOnly, iNode);
                supportedFactory.Fields.Add(currentField);

                var kernelField = new FieldDefinition("_kernel", FieldAttributes.Private | FieldAttributes.InitOnly, iKernel);
                supportedFactory.Fields.Add(kernelField);

                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.CompilerControlled |
                        MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.RTSpecialName,
                    type.Module.Import(typeof(void)));
                ctor.Body.InitLocals = true;
                supportedFactory.Methods.Add(ctor);

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

                type.Module.Types.Add(supportedFactory);
                
                foreach (var method in type.Methods)
                {
                    var impl = new MethodDefinition(method.Name,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual,
                        method.ReturnType);
                    supportedFactory.Methods.Add(impl);
                    
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

                    var realReturnType = method.ReturnType;
                    var kernelGetMethod = kernelGet;
                    var isAsynchronous = false;
                    if (method.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task`1"))
                    {
                        realReturnType = ((GenericInstanceType)method.ReturnType).GenericArguments.First();
                        var genericMethodInstance = new GenericInstanceMethod(kernelGetAsync);
                        genericMethodInstance.GenericArguments.Add(realReturnType);
                        kernelGetMethod = genericMethodInstance;
                        isAsynchronous = true;
                    }

                    impl.Body.InitLocals = true;

                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, kernelField));
                    if (!isAsynchronous)
                    {
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, realReturnType));
                        impl.Body.Instructions.Add(Instruction.Create(OpCodes.Call, getTypeFromHandle));
                    }
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, currentField));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, 0));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Newarr, iInjectionAttribute));
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
                    
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));

                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, kernelGetMethod));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Castclass, method.ReturnType));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                }
                
                var notSupportedFactory = new TypeDefinition(
                    "_GeneratedFactories",
                    "NotSupportedGenerated" + type.Name.Substring(1),
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit);
                notSupportedFactory.BaseType = @object;

                foreach (var gp in type.GenericParameters)
                {
                    var ngp = new GenericParameter(gp.Name, notSupportedFactory);
                    ngp.Attributes = gp.Attributes;
                    foreach (TypeReference gpc in gp.Constraints)
                        ngp.Constraints.Add(gpc);
                    notSupportedFactory.GenericParameters.Add(ngp);
                }

                if (notSupportedFactory.GenericParameters.Count > 0)
                {
                    notSupportedFactory.Interfaces.Add(type.MakeGenericInstanceType(notSupportedFactory.GenericParameters.Cast<TypeReference>().ToArray()));
                }
                else
                {
                    notSupportedFactory.Interfaces.Add(type);
                }

                ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.CompilerControlled |
                        MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                        MethodAttributes.RTSpecialName,
                    type.Module.Import(typeof(void)));
                ctor.Body.InitLocals = true;
                notSupportedFactory.Methods.Add(ctor);

                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, objectConstructor));
                ctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                type.Module.Types.Add(notSupportedFactory);

                foreach (var method in type.Methods)
                {
                    var impl = new MethodDefinition(method.Name,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                        MethodAttributes.NewSlot | MethodAttributes.Virtual,
                        method.ReturnType);
                    notSupportedFactory.Methods.Add(impl);

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

                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, notSupportedExceptionCtor));
                    impl.Body.Instructions.Add(Instruction.Create(OpCodes.Throw));
                }

                var attribute = new CustomAttribute(generatedFactoryAttributeConstructor);
                attribute.ConstructorArguments.Add(new CustomAttributeArgument(@string, supportedFactory.FullName));
                attribute.ConstructorArguments.Add(new CustomAttributeArgument(@string, notSupportedFactory.FullName));
                type.CustomAttributes.Add(attribute);
                
                modified = true;
            }

            if (modified)
            {
                Console.WriteLine("Saving assembly: " + args[0]);
                assembly.Write(args[0], new WriterParameters {WriteSymbols = readSymbols });
            }
        }
    }
}