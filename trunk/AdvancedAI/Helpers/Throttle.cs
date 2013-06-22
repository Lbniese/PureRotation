﻿using System;
using System.Collections.Generic;
using Styx.TreeSharp;

namespace AdvancedAI.Helpers
{
    /// <summary>
    ///   Implements a 'throttle' composite. This composite limits the number of times the child 
    ///   will be run within a given time span.  Returns cappedStatus if limit reached, otherwise
    ///   Returns result of child
    /// </summary>
    /// <remarks>
    ///   Created 10/28/2012.
    /// </remarks>
    public class ThrottlePasses : Decorator
    {
        private DateTime _end;
        private int _count;
        private RunStatus _limitStatus;

        /// <summary>
        /// time span that Limit child Successes can occur
        /// </summary>
        public TimeSpan TimeFrame { get; set; }
        /// <summary>
        /// maximum number of child Successes that can occur within TimeFrame
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        ///   Implements a 'throttle' composite. This composite limits the number of times the child 
        ///   will be run within a given time span.  Returns cappedStatus for attempts after limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "limit">max number of occurrences</param>
        /// <param name = "timeFrame">time span for occurrences</param>
        /// <param name="limitStatus">RunStatus to return when limit reached</param>
        /// <param name = "child">composite children to tick (run)</param>
        public ThrottlePasses(int limit, TimeSpan timeFrame, RunStatus limitStatus, Composite child)
            : base(child)
        {
            TimeFrame = timeFrame;
            Limit = limit;

            _end = DateTime.MinValue;
            _count = 0;
            _limitStatus = limitStatus;
        }

        /// <summary>
        ///   Implements a 'throttle' composite. This composite limits the number of times the child 
        ///   to running once within a given time span.  Returns Failure if attempted to run after
        ///   limit reached in timeframe, otherwise returns result of child
        /// </summary>
        /// <param name = "timeFrame">wait TimeSpan after child success before another attempt</param>
        /// <param name = "child">composite children to tick (run)</param>
        public ThrottlePasses(TimeSpan timeFrame, Composite child)
            : this(1, timeFrame, RunStatus.Failure, child)
        {
        }

        /// <summary>
        ///   Implements a 'throttle' composite. This composite limits the number of times the child 
        ///   will be run within a given time span.  Returns Failure for attempts after limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "Limit">max number of occurrences</param>
        /// <param name = "timeFrame">time span for occurrences in seconds</param>
        /// <param name = "child">composite children to tick (run)</param>
        public ThrottlePasses(int Limit, int timeSeconds, Composite child)
            : this(Limit, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, child)
        {

        }

        public ThrottlePasses(int Limit, TimeSpan ts, Composite child)
            : this(Limit, ts, RunStatus.Failure, child)
        {

        }

        /// <summary>
        ///   Implements a 'throttle' composite. This composite limits the number of times the child 
        ///   will be run within a given time span.  Returns Failure if limit reached, otherwise
        ///   Returns result of child
        /// </summary>
        /// <param name = "timeFrame">time span for occurrences in seconds</param>
        /// <param name = "child">composite children to tick (run)</param>
        public ThrottlePasses(int timeSeconds, Composite child)
            : this(1, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, child)
        {

        }

        public override void Start(object context)
        {
            base.Start(context);
        }

        public override void Stop(object context)
        {
            base.Stop(context);
        }

        protected override IEnumerable<RunStatus> Execute(object context)
        {
            if (DateTime.Now < _end && _count >= Limit)
            {
                yield return _limitStatus;
                yield break;
            }

            DecoratedChild.Start(context);

            RunStatus childStatus;
            while ((childStatus = DecoratedChild.Tick(context)) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);

            if (DateTime.Now > _end)
            {
                _count = 0;
                _end = DateTime.Now + TimeFrame;
            }

            _count++;

            if (DecoratedChild.LastStatus == RunStatus.Failure)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            yield return RunStatus.Success;
            yield break;
        }
    }



    /// <summary>
    ///   Implements a 'throttle' composite. This composite limits the number of times the child 
    ///   returns RunStatus.Success within a given time span.  Returns cappedStatus if limit reached, 
    ///   otherwise returns result of child
    /// </summary>
    /// <remarks>
    ///   Created 10/28/2012.
    /// </remarks>
    public class Throttle : Decorator
    {
        private DateTime _end;
        private int _count;
        private RunStatus _limitStatus;

        /// <summary>
        /// time span that Limit child Successes can occur
        /// </summary>
        public TimeSpan TimeFrame { get; set; }
        /// <summary>
        /// maximum number of child Successes that can occur within TimeFrame
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success within a given time span.  Returns cappedStatus if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "limit">max number of occurrences</param>
        /// <param name = "timeFrame">time span for occurrences</param>
        /// <param name="limitStatus">RunStatus to return when limit reached</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(int limit, TimeSpan timeFrame, RunStatus limitStatus, Composite child)
            : base(child)
        {
            TimeFrame = timeFrame;
            Limit = limit;

            _end = DateTime.MinValue;
            _count = 0;
            _limitStatus = limitStatus;
        }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success within a given time span.  Returns Failure if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "timeFrame">time span for occurrences</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(TimeSpan timeFrame, Composite child)
            : this(1, timeFrame, RunStatus.Failure, child)
        {
        }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success within a given time span.  Returns Failure if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "timeFrame">time span for occurrences</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(int Limit, TimeSpan timeFrame, Composite child)
            : this(Limit, timeFrame, RunStatus.Failure, child)
        {
        }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success within a given time span.  Returns Failure if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "Limit">max number of occurrences</param>
        /// <param name = "timeFrame">time span for occurrences in seconds</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(int Limit, int timeSeconds, Composite child)
            : this(Limit, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, child)
        {
            
        }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success within a given time span.  Returns Failure if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "timeFrame">wait in seconds after child success before another attempt</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(int timeSeconds, Composite child)
            : this(1, TimeSpan.FromSeconds(timeSeconds), RunStatus.Failure, child)
        {
            
        }

        /// <summary>
        ///   Creates a 'throttle' composite. This composite limits the number of times the child 
        ///   returns RunStatus.Success to once per 250ms.  Returns Failure if limit reached, 
        ///   otherwise returns result of child
        /// </summary>
        /// <param name = "timeFrame">time span for occurrences in seconds</param>
        /// <param name = "child">composite children to tick (run)</param>
        public Throttle(Composite child)
            : this(1, TimeSpan.FromMilliseconds(250), RunStatus.Failure, child)
        {

        }

        public override void Start(object context)
        {
            base.Start(context);
        }

        public override void Stop(object context)
        {
            base.Stop(context);
        }

        protected override IEnumerable<RunStatus> Execute(object context)
        {
            if (DateTime.Now < _end && _count >= Limit)
            {
                yield return _limitStatus;
                yield break;
            }

            // check not present in Decorator, but adding here
            if (DecoratedChild == null)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            DecoratedChild.Start(context);

            RunStatus childStatus;
            while ((childStatus = DecoratedChild.Tick(context)) == RunStatus.Running)
            {
                yield return RunStatus.Running;
            }

            DecoratedChild.Stop(context);

            if (DecoratedChild.LastStatus == RunStatus.Failure)
            {
                yield return RunStatus.Failure;
                yield break;
            }

            if (DateTime.Now > _end)
            {
                _count = 0;
                _end = DateTime.Now + TimeFrame;
            }

            _count++;

            yield return RunStatus.Success;
            yield break;
        }
    }


}
