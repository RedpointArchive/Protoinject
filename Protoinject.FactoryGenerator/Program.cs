using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Protoinject.FactoryGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var assembly = AssemblyDefinition.ReadAssembly(args[0], new ReaderParameters {ReadSymbols = true});
            Console.WriteLine("Generating factories for " + assembly.FullName + "...");

            var iGenerateFactory = assembly.MainModule.Import(typeof (IGenerateFactory));
            var iNode = assembly.MainModule.Import(typeof (INode));
            var iKernel = assembly.MainModule.Import(typeof (IKernel));
            var iCurrentNode = assembly.MainModule.Import(typeof(ICurrentNode));
            var @object = assembly.MainModule.Import(typeof(object));
            var @string = assembly.MainModule.Import(typeof(string));
            var objectConstructor = assembly.MainModule.Import(typeof(object).GetConstructor(new Type[0]));
            var getNodeForFactoryImplementation = 
                assembly.MainModule.Import(typeof(ICurrentNode).GetMethod("GetNodeForFactoryImplementation"));
            var getTypeFromHandle = assembly.MainModule.Import(typeof (Type).GetMethod("GetTypeFromHandle"));
            var iConstructorArgument = assembly.MainModule.Import(typeof(IConstructorArgument));
            var namedConstructorArgumentConstructor =
                assembly.MainModule.Import(
                    typeof (NamedConstructorArgument).GetConstructor(new Type[] {typeof (string), typeof (object)}));
            var kernelGet = assembly.MainModule.Import(
                typeof (IKernel).GetMethod("Get",
                    new Type[]
                    {typeof (Type), typeof (INode), typeof (string), typeof (string), typeof (IConstructorArgument[])}));
            var generatedFactoryAttributeConstructor = assembly.MainModule.Import(
                typeof(GeneratedFactoryAttribute).GetConstructor(new []{typeof(string)}));

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
                    "Generated" + type.Name.Substring(1) + "<>",
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit);
                factory.BaseType = @object;
                factory.Interfaces.Add(type);

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
                assembly.Write(args[0], new WriterParameters {WriteSymbols = true});
            }
        }
    }
}