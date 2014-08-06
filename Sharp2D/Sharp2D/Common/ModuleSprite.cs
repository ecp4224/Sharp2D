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
        private List<IModule> toRemove = new List<IModule>();
        private List<IModule> toAdd = new List<IModule>();
        private object m_lock = new object();
        private bool looping = false;
        private int tID;

        private List<IModule> _modules = new List<IModule>();
        
        
        public virtual void Update()
        {
            lock (m_lock)
            {
                looping = true;
                tID = Thread.CurrentThread.ManagedThreadId;
                
                foreach (IModule m in _modules)
                {
                    m.OnUpdate();

                    if ((m.Rules & ModuleRules.RunOnce) != 0)
                        RemoveModule(m);
                }
                
                looping = false;

                foreach (IModule m in toAdd)
                {
                    _modules.Add(m);

                    m.InitializeWith(this);

                    Logger.Debug("Module " + m.ModuleName + " attached to " + ToString() + ".");
                }
                toAdd.Clear();

                foreach (IModule m in toRemove)
                {
                    _modules.Remove(m);
                }

                toRemove.Clear();
            }
        }

        protected override void OnDispose()
        {
            if (looping)
            {
                if (Thread.CurrentThread.ManagedThreadId == tID)
                {
                    Logger.Error("The Update Thread is attempting to dispose a module sprite while updating its own modules!");
                    Logger.Error("The modules can't be disposed properly! It is recommended you debug why this is occuring!");
                    Logger.PrintStackTrace();
                    return;
                }
            }

            lock (m_lock)
            {
                _modules.Clear();
                toAdd.Clear();
                toRemove.Clear();
            }
        }

        public T AttachModule<T>() where T : IModule
        {
            T m = (T)Activator.CreateInstance(typeof(T));

            AttachModule(m);

            return m;
        }

        public T AttachModule<T>(params object[] args) where T : IModule
        {
            T m = (T)Activator.CreateInstance(typeof(T), args);

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

            if (!looping)
            {
                _modules.Add(m);

                m.InitializeWith(this);

                Logger.Debug("Module " + m.ModuleName + " attached to " + ToString() + ".");
            }
            else
                toAdd.Add(m);
        }

        public void RemoveModule(IModule m)
        {
            if (!looping)
                _modules.Remove(m);
            else
                toRemove.Remove(m);
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
            foreach (IModule m in _modules)
            {
                if (m is T) return true;
            }

            foreach (IModule m in toAdd)
            {
                if (m is T) return true;
            }

            return false;
        }

        public bool ModuleExists(IModule m)
        {
            return ModuleExists(m.GetType());
        }

        public bool ModuleExists(Type T)
        {
            foreach (IModule m in _modules)
            {
                if (m.GetType() == T) return true;
            }

            foreach (IModule m in toAdd)
            {
                if (m.GetType() == T) return true;
            }

            return false;
        }

        public List<T> GetModules<T>() where T : IModule
        {
            return _modules.OfType<T>().Concat<T>(toAdd.OfType<T>()).ToList<T>();
        }

        public T GetFirstModule<T>() where T : IModule
        {
            return GetModules<T>()[0];
        }
    }
}
