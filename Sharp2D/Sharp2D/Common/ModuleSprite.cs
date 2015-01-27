using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sharp2D.Core.Interfaces;

namespace Sharp2D
{
    public abstract class ModuleSprite : Sprite, ILogical
    {
        private readonly List<IModule> _toRemove = new List<IModule>();
        private readonly List<IModule> _toAdd = new List<IModule>();
        private readonly object _mLock = new object();
        private bool _looping = false;
        private int _tId;

        private readonly List<IModule> _modules = new List<IModule>();
        
        
        public virtual void Update()
        {
            lock (_mLock)
            {
                _looping = true;
                _tId = Thread.CurrentThread.ManagedThreadId;
                
                foreach (IModule m in _modules)
                {
                    m.OnUpdate();

                    if ((m.Rules & ModuleRules.RunOnce) != 0)
                        RemoveModule(m);
                }
                
                _looping = false;

                foreach (IModule m in _toAdd)
                {
                    _modules.Add(m);

                    m.InitializeWith(this);

                    Logger.Debug("Module " + m.ModuleName + " attached to " + ToString() + ".");
                }
                _toAdd.Clear();

                foreach (IModule m in _toRemove)
                {
                    _modules.Remove(m);
                }

                _toRemove.Clear();
            }
        }

        protected override void OnDispose()
        {
            if (_looping)
            {
                if (Thread.CurrentThread.ManagedThreadId == _tId)
                {
                    Logger.Error("The Update Thread is attempting to dispose a module sprite while updating its own modules!");
                    Logger.Error("The modules can't be disposed properly! It is recommended you debug why this is occuring!");
                    Logger.PrintStackTrace();
                    return;
                }
            }

            lock (_mLock)
            {
                _modules.Clear();
                _toAdd.Clear();
                _toRemove.Clear();
            }
        }

        public T AttachModule<T>() where T : IModule, new()
        {
            var m = new T();

            AttachModule(m);

            return m;
        }

        public T AttachModule<T>(params object[] args) where T : IModule, new()
        {
            var m = new T();

            AttachModule(m);

            return m;
        }

        public void AttachModule(IModule m)
        {
            if ((m.Rules & ModuleRules.OnePerSprite) != 0 && ModuleExists(m))
            {
                Logger.Warn("Module " + m.ModuleName + " not added to Sprite " + ToString() + " because it already exists, and module only allows one per Sprite!");
                return;
            }

            if (!_looping)
            {
                _modules.Add(m);

                m.InitializeWith(this);

                Logger.Debug("Module " + m.ModuleName + " attached to " + ToString() + ".");
            }
            else
                _toAdd.Add(m);
        }

        public void RemoveModule(IModule m)
        {
            if (!_looping)
                _modules.Remove(m);
            else
                _toRemove.Remove(m);
        }

        public void RemoveModule<T>() where T : IModule
        {
            foreach (var m in _modules.OfType<T>())
            {
                RemoveModule(m);
            }
        }

        public bool ModuleExists<T>() where T : IModule
        {
            return _modules.OfType<T>().Any() || _toAdd.OfType<T>().Any();
        }

        public bool ModuleExists(IModule m)
        {
            return _modules.Contains(m) || _toAdd.Contains(m);
        }

        public bool ModuleExists(Type T)
        {
            return _modules.Any(m => m.GetType() == T) || _toAdd.Any(m => m.GetType() == T);
        }

        public List<T> GetModules<T>() where T : IModule
        {
            return _modules.OfType<T>().Concat<T>(_toAdd.OfType<T>()).ToList<T>();
        }

        public T GetFirstModule<T>() where T : IModule
        {
            return GetModules<T>()[0];
        }
    }
}
