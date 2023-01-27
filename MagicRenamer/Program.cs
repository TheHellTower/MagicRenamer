using dnlib.DotNet;
using System;
using System.Linq;

namespace MagicRenamer
{
    internal class Program
    {
        static ModuleDefMD Module = null;
        
        private static Random rnd = new Random();
        static string dnSpyFriendlyName = "<TheHellTower>"; //dnSpy love this

        static void Main(string[] args)
        {
            Module = ModuleDefMD.Load(args[0]);
            MagicRenamer(Module);
            Module.Write(Module.Location.Insert(Module.Location.Length-4, "-magic"));
        }

        private static void MagicRenamer(ModuleDefMD module)
        {
            string text = null;
            foreach (TypeDef typeDef in module.Types.Where(t => !t.IsGlobalModuleType && !t.Namespace.Contains("My") && t.Interfaces.Count == 0 && !t.IsSpecialName && !t.IsRuntimeSpecialName))
            {
                if (typeDef.Namespace != $"{Module.Assembly.Name}.Properties") //They don't get hidden so it's useless.
                {
                    if (typeDef.IsPublic)
                        text = typeDef.Name;

                    if (!typeDef.Name.Contains("PrivateImplementationDetails"))
                        typeDef.Name = $"<{typeDef.Name}>";

                    //foreach (MethodDef methodDef in typeDef.Methods.Where(m => !m.IsConstructor && !m.IsStaticConstructor && !m.DeclaringType.IsForwarder && !m.IsFamily && !m.IsRuntimeSpecialName && !m.DeclaringType.IsForwarder && !m.DeclaringType.IsGlobalModuleType))
                    foreach (MethodDef methodDef in typeDef.Methods.Where(m => !m.DeclaringType.IsForwarder && !m.IsFamily && !m.IsRuntimeSpecialName && !m.DeclaringType.IsForwarder))
                    {
                        methodDef.CustomAttributes.Add(new CustomAttribute(new MemberRefUser(module, "THT", MethodSig.CreateInstance(module.Import(typeof(void)).ToTypeSig(true)), module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CompilerGeneratedAttribute"))));
                        methodDef.Name = dnSpyFriendlyName; //dnSpy love this combo
                        foreach (Parameter parameter in methodDef.Parameters)
                            parameter.Name = dnSpyFriendlyName;
                        if (typeDef.Name.Contains(methodDef.Name))
                            methodDef.Name = typeDef.Name;
                    }
                    
                    foreach (FieldDef fieldDef in typeDef.Fields.Where(f => !f.DeclaringType.IsEnum && !f.DeclaringType.IsForwarder && !f.IsRuntimeSpecialName && !f.DeclaringType.IsEnum))
                        fieldDef.Name = dnSpyFriendlyName;

                    foreach (EventDef eventDef in typeDef.Events.Where(e => !e.DeclaringType.IsForwarder && !e.IsRuntimeSpecialName))
                        eventDef.Name = dnSpyFriendlyName;

                    if (typeDef.IsPublic)
                        foreach (Resource resource in module.Resources.Where(r => r.Name.Contains(text)))
                            resource.Name = resource.Name.Replace(text, typeDef.Name); //Fix resources
                }
            }
        }
    }
}