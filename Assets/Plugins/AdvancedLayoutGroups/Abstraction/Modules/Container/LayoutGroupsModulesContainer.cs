using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedLayoutGroups.Abstraction.Modules.Container
{
    [Serializable]
    public class LayoutGroupsModulesContainer : ILayoutGroupsModulesContainerReadonly
    {
        public IEnumerable<LayoutGroupModuleBase> Modules => _modules.Select(a => (LayoutGroupModuleBase)a);
        
        public int ModulesCount => _modules.Count;

        private Dictionary<Type, object> ModulesMap
        {
            get
            {
                if (_modulesMap == null)
                {
                    RecreateModulesMap();
                }

                return _modulesMap;
            }
        }

        private List<object> _modules = new List<object>();
        
        private Dictionary<Type, object> _modulesMap;
        private Dictionary<object, List<Type>> _bindingTypesMap = new Dictionary<object, List<Type>>();
        
        public void Add<T>(T module)
        {
            BindByType(module, module.GetType());
            _modules.Add(module);
        }

        public void Add<T>(T module, params Type[] additionalBindingTypes)
        {
            BindByType(module, module.GetType());

            foreach (var type in additionalBindingTypes)
            {
                BindByType(module, type);
            }
            
            _modules.Add(module);
        }

        private void BindByType(object module, Type type)
        {
            if (ModulesMap.ContainsKey(type))
            {
                ThrowModuleOfTypeAlreadyExists(module);
            }

            if (_bindingTypesMap.ContainsKey(module) == false)
            {
                _bindingTypesMap.Add(module, new List<Type>());
            }
            
            _bindingTypesMap[module].Add(type);
            ModulesMap.Add(type, module);
        }

        public bool Remove<T>() where T : LayoutGroupModuleBase
        {
            T module = Get<T>();
            return Remove(module);
        }
        
        public bool Remove<T>(T module) where T : LayoutGroupModuleBase
        {
            bool removed = _modules.Remove(module);

            if (removed)
            {
                List<Type> allBindingTypes = _bindingTypesMap[module];

                foreach (var type in allBindingTypes)
                {
                    ModulesMap.Remove(type);
                }

                _bindingTypesMap.Remove(module);
            }
   
            return removed;
        }

        public T Get<T>()
        {
            TryGet(out T module);
            return module;
        }

        public bool TryGet<T>(out T module)
        {
            Type type = typeof(T);

            module = default;

            if (ModulesMap.ContainsKey(type) == false)
            {
                return false;
            }

            module = (T)ModulesMap[type];
            return true;
        }

        public bool Has<T>()
        {
            return ModulesMap.ContainsKey(typeof(T));
        }

        public void Update(float deltaTime)
        {
            foreach (var module in _modules)
            {
                if (module is IUpdatableLayoutModule updatableLayoutModule)
                {
                    updatableLayoutModule.Update(deltaTime);
                }
            }
        }

        private void RecreateModulesMap()
        {
            if (_modulesMap == null)
            {
                _modulesMap = new Dictionary<Type, object>(_modules.Count);
            }
            else
            {
                _modulesMap.Clear();
            }

            foreach (var module in _modules)
            {
                if (module == null) continue;

                _modulesMap.Add(module.GetType(), module);
            }
        }

        private void ThrowModuleOfTypeAlreadyExists<T>(T module)
        {
            throw new InvalidOperationException($"{module.GetType()} type of module already exists in the container");
        }
    }
}