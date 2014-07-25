using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;
using Sharp2D.Core.Interfaces;

namespace Sharp2D.Core
{
    public abstract class World : ILogicContainer, IRenderJobContainer
    {
        ~World()
        {
            Dispose();
        }

        protected List<ILogical> logics;
        private List<ILogical> lCache;
        private List<ILogical> lToRemove;

        protected List<IRenderJob> jobs;
        private List<IRenderJob> jCache;
        private List<IRenderJob> jToRemove;
        private bool lFetch;

        public List<ILogical> LogicalList
        {
            get
            {
                if (!lFetch)
                    throw new AccessViolationException("PreFetch() was not invoked. Not ready for fetch!");
                if (!IsDisposing)
                    return logics;
                return null;
            }
        }

        public List<IRenderJob> RenderJobs
        {
            get
            {
                if (!lFetch)
                    throw new AccessViolationException("PreFetch() was not invoked. Not ready for fetch!");
                if (!IsDisposing)
                    return jobs;
                return null;
            }
        }

        public bool Loaded { get; private set; }

        public bool Displaying { get; private set; }

        public bool IsDisposing { get; private set; }

        public abstract string Name { get; }

        protected abstract void OnLoad();

        protected abstract void OnDisplay();

        protected abstract void OnUnload();

        protected abstract void OnDispose();

        public void Display()
        {
            Screen.ValidateOpenGLUnsafe("Display()");

            Screen.LogicContainer = this;
            Screen.RenderJobContainer = this;
            Displaying = true;
        }

        public void Load()
        {
            logics = new List<ILogical>();
            jobs = new List<IRenderJob>();
            lCache = new List<ILogical>();
            jCache = new List<IRenderJob>();
            lToRemove = new List<ILogical>();
            jToRemove = new List<IRenderJob>();

            OnLoad();

            Loaded = true;
        }

        public void Unload()
        {
            Screen.ValidateOpenGLUnsafe("Unload()");

            OnUnload();
            Screen.LogicContainer = null;
            Screen.RenderJobContainer = null;
            Displaying = false;
        }

        public void Dispose()
        {
            Screen.ValidateOpenGLUnsafe("Dispose()");

            IsDisposing = true;
            if (Displaying)
                Unload();

            OnDispose();

            _removeLogicalScheduled();
            _removeJobScheduled();
            _addLogicalScheduled();
            _addJobScheduled();

            if (logics != null)
            {
                foreach (ILogical logical in logics)
                {
                    logical.Dispose();
                }
                logics.Clear();
            }
            if (jobs != null)
            {
                foreach (IRenderJob job in jobs)
                {
                    job.Dispose();
                }
                jobs.Clear();
            }
            
            logics = null;
            jobs = null;
            lCache = null;
            jCache = null;
            lToRemove = null;
            jToRemove = null;

            IsDisposing = false;
            Loaded = false;
        }

        public void AddRenderJob(IRenderJob job)
        {
            if (job == null)
                throw new ArgumentException("Job cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not add job..");

            if (!lFetch)
                jobs.Add(job);
            else
                jCache.Add(job);
        }

        public void AddRenderJobAt(IRenderJob job, int index)
        {
            if (job == null)
                throw new ArgumentException("Job cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not add job..");


            jobs.Insert(index, job);
        }

        public void RemoveRenderJob(IRenderJob job)
        {
            if (job == null)
                throw new ArgumentException("Job cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before removing jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not remove job..");

            if (!lFetch)
                jobs.Remove(job);
            else
            {
                if (jCache.Contains(job))
                    jCache.Remove(job);
                else
                    jToRemove.Add(job);
            }
        }

        public void AddLogical(ILogical logical)
        {
            if (logical == null)
                throw new ArgumentException("Logical cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not add logical item..");

            if (!lFetch)
                logics.Add(logical);
            else
                lCache.Add(logical);
        }

        public void AddLogical(Action action)
        {
            if (action == null)
                throw new ArgumentException("Action cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not add logical item..");

            DummyLogical logical = new DummyLogical(action);

            if (!lFetch)
                logics.Add(logical);
            else
                lCache.Add(logical);
        }

        public void RemoveLogical(ILogical logical)
        {
            if (logical == null)
                throw new ArgumentException("Logical cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not remove logical item..");

            if (!lFetch)
                logics.Remove(logical);
            else
            {
                if (lCache.Contains(logical))
                    lCache.Remove(logical);
                else
                {
                    if (logics.Contains(logical))
                        lToRemove.Add(logical);
                }
            }

        }

        public bool HasJob(IRenderJob job)
        {
            if (job == null)
                throw new ArgumentException("Job cannot be null!");
            return jobs.Contains(job) || jCache.Contains(job);
        }


        public void PreFetch()
        {
            lFetch = true;
            if (!used)
            {
                used = true;
                OnDisplay();
            }
        }

        public void PostFetch()
        {
            lFetch = false;
            _removeLogicalScheduled();
            _removeJobScheduled();
            _addLogicalScheduled();
            _addJobScheduled();
        }

        private void _addLogicalScheduled()
        {
            foreach (ILogical l in lCache)
            {
                logics.Add(l);
            }
            lCache.Clear();
        }

        private void _removeLogicalScheduled()
        {
            foreach (ILogical l in lToRemove)
            {
                logics.Remove(l);
            }
            lToRemove.Clear();
        }

        private void _addJobScheduled()
        {
            foreach (IRenderJob j in jCache)
            {
                jobs.Add(j);
            }
            jCache.Clear();
        }

        private void _removeJobScheduled()
        {
            foreach (IRenderJob j in jToRemove)
            {
                jobs.Remove(j);
            }
            jToRemove.Clear();
        }


        private bool used = false;
    }

    internal class DummyLogical : ILogical
    {
        Action callback;

        public DummyLogical(Action callback)
        {
            this.callback = callback;
        }

        public void Update()
        {
            callback();
        }

        public void Dispose()
        {
        }
    }
}
