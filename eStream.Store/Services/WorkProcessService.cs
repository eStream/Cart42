using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Estream.Cart42.Web.DAL;
using Estream.Cart42.Web.Domain;

namespace Estream.Cart42.Web.Services
{
    public class WorkProcessService : IWorkProcessService
    {
        private readonly DataContext db;

        public WorkProcessService(DataContext db)
        {
            this.db = db;
        }

        public int Add(WorkProcessType type)
        {
            var workProcess = new WorkProcess {Type = type};

            db.WorkProcesses.Add(workProcess);
            db.SaveChanges();

            return workProcess.Id;
        }

        public void Update(int id, string status, double percentageCompleted, bool? isComplete = null, string error = null)
        {
            bool cancelRequested;
            Update(id, status, percentageCompleted, out cancelRequested, isComplete, error);
        }

        public void Update(int id, string status, double percentageCompleted, out bool cancelRequested, 
            bool? isComplete = null, string error = null)
        {
            var workProcess = db.WorkProcesses.Find(id);
            if (workProcess == null)
            {
                cancelRequested = true;
                return;
            }

            cancelRequested = workProcess.CancelRequested;
            if (!workProcess.IsRunning) workProcess.IsRunning = true;
            workProcess.Status = status;
            workProcess.PercentComplete = percentageCompleted;

            if (isComplete.HasValue && isComplete == true)
            {
                workProcess.IsComplete = true;
                workProcess.IsRunning = false;
            }

            if (error != null)
            {
                workProcess.Error = error;
            }

            db.SaveChanges();
        }

        public IQueryable<WorkProcess> Find(WorkProcessType type)
        {
            var cutoffTime = DateTime.Now.Subtract(TimeSpan.FromHours(6));
            return db.WorkProcesses.Where(p => p.Type == type && p.DateCreated >= cutoffTime);
        }

        public void Cancel(int id)
        {
            var workProcess = db.WorkProcesses.Find(id);
            if (workProcess == null) return;

            workProcess.CancelRequested = true;

            db.SaveChanges();
        }

        public void Delete(int id)
        {
            var workProcess = db.WorkProcesses.Find(id);
            if (workProcess == null) return;

            db.WorkProcesses.Remove(workProcess);

            db.SaveChanges();
        }
    }
}