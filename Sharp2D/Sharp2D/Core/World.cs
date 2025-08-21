﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Interfaces;
using Sharp2D.Game.Worlds;

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

        public event EventHandler OnWorldLoaded;

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

        public Camera Camera { get; set; }

        public bool Loaded { get; private set; }

        public bool Displaying { get; private set; }

        public bool IsDisposing { get; private set; }

        public abstract string Name { get; }

        /// <summary>
        /// This method is invoked when World.Load() is invoked
        /// </summary>
        protected abstract void OnLoad();

        /// <summary>
        /// This method is invoked after World.Load() has completed fully
        /// </summary>
        protected virtual void OnLoadCompleted()
        {
            OnWorldLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// This method is invoked when the first frame of this World is about to be displayed.
        /// This method is only ever invoked once during the life cycle, and will can only ever be reinvoked if World.Unload() was invoked
        /// </summary>
        protected abstract void OnInitialDisplay();

        /// <summary>
        /// This method is invoked when the World is displayed on the screen once again after being backgrouned by another World.
        /// </summary>
        protected abstract void OnResumeDisplay();

        /// <summary>
        /// This method is invoked when this World is being removed from the Screen by another World, or being put in the "background"
        /// </summary>
        protected abstract void OnBackgroundDisplay();

        /// <summary>
        /// This method is invoked when World.Unload() is invoked.
        /// </summary>
        protected abstract void OnUnload();

        /// <summary>
        /// This method is invoked when World.Dispose() is invoked, or when this World is reclaimed by garbage collection.
        /// </summary>
        protected abstract void OnDispose();

        public void Display()
        {
            Screen.ValidateOpenGLUnsafe("Display()");

            var currentWorld = Screen.LogicContainer as World ?? Screen.RenderJobContainer as World;

            if (currentWorld != null)
            {
                currentWorld.Displaying = false;
                currentWorld.OnBackgroundDisplay();
            }

            if (used) //This variable will be true if OnInitialDisplay was invoked
            {
                OnResumeDisplay();
            }

            Screen.LogicContainer = this;
            Screen.RenderJobContainer = this;
            Displaying = true;

            while (!used) { System.Threading.Thread.Sleep(50); }
        }

        public void Load()
        {
            logics = new List<ILogical>();
            jobs = new List<IRenderJob>();
            lCache = new List<ILogical>();
            jCache = new List<IRenderJob>();
            lToRemove = new List<ILogical>();
            jToRemove = new List<IRenderJob>();
            Camera = new GenericCamera();

            OnLoad();

            Loaded = true;
            
            OnLoadCompleted();
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
            if (IsDisposed || IsDisposing)
                return;

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

            IsDisposed = true;
            IsDisposing = false;
            Loaded = false;
        }

        public bool IsDisposed { get; private set; }

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

        public ILogical AddLogical(Action action)
        {
            if (action == null)
                throw new ArgumentException("Action cannot be null!");
            if (!Loaded)
                throw new InvalidOperationException("This world object has not been loaded! Please call the \"Load()\" method before adding jobs!");
            if (IsDisposing)
                throw new InvalidOperationException("This world object is currently disposing, can not add logical item..");

            var logical = new DummyLogical(action);

            if (!lFetch)
                logics.Add(logical);
            else
                lCache.Add(logical);

            return logical;
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

        public T GetJob<T>() where T : IRenderJob
        {
            return jobs.OfType<T>().FirstOrDefault();
        }

        public void PreFetch()
        {
            lFetch = true;
            if (!used)
            {
                used = true;
                OnInitialDisplay();
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
        readonly Action _callback;

        public DummyLogical(Action callback)
        {
            _callback = callback;
        }

        public void Update()
        {
            _callback();
        }

        public void Dispose()
        {
        }
    }
}
