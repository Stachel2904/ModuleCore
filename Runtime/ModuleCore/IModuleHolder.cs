using System;

namespace DivineSkies.Modules.Core
{
    public interface IModuleHolder
    {
        public Type[] GetConstantModuleTypes();
    }
}
