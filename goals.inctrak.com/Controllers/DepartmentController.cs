using IncTrak.GoalSetter.data;
using IncTrak.GoalSetter.Data;
using IncTrak.GoalSetter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace IncTrak.GoalSetter.Controllers
{
    [ApiController]
    public class DepartmentController : IncTrakController
    {
        public DepartmentController(IOptions<AppSettings> config) : base(config)
        {
        }

        private object[] QueryDepartments(inctrak_goalsContext context, LoginRights rights)
        {
            IQueryable<Departments> query = from d in context.Departments
                                              where (d.GroupFk == rights.GroupKey || d.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                            select d;

            var existingStokcClass = query.Select(d => new DEPARTMENTS_UI(rights.GroupKeyCheck) { SetFromDepartment = d, CURRENT_GOALS = d.Teams.Sum(t => t.Goalset.Where(g=> g.ScheduleFkNavigation.EndDate > DateTime.Now).Sum(g=> g.Goals.Count())), TOTAL_GOALS = d.Teams.Sum(t => t.Goalset.Sum(g => g.Goals.Count())) }).ToArray();
            if (rights.IsAdmin)
            {
                var newDepartment = new DEPARTMENTS_UI[1] { new DEPARTMENTS_UI(rights.GroupKeyCheck, new Departments() { DepartmentPk = Guid.Empty, GroupFk = rights.GroupKeyCheck, Name = "<Create New Department>" }) };

                return newDepartment.Union(existingStokcClass).ToArray();
            }

            return existingStokcClass.ToArray();
        }

        [Route("api/company/departments/")]
        public object GetDepartments()
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, Guid.Empty.ToString());
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    return QueryDepartments(context, rights);
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get department");

                return new { success = false, message = message };
            }
        }

        [Route("api/company/department/{departmentKey}/{uuidKey}")]
        public object GetDepartment(Guid departmentKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    Departments department;
                    if (departmentKey == Guid.Empty)
                        department = new Departments() { GroupFk = rights.GroupKeyCheck };
                    else
                        department = (from sc in context.Departments
                                    where sc.DepartmentPk == departmentKey && (sc.GroupFk == rights.GroupKey || sc.GroupFkNavigation.Users.Any(u => u.UserPk == rights.UserKey))
                                     select sc).FirstOrDefault();
                    if (department != null)
                        return new { success = true, Department = new DEPARTMENTS_UI(rights.GroupKeyCheck, department) };
                }
                throw new Exception("Cannot find department");
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "get a department");
                return new { success = false, message = message }; ;
            }
        }

        [Route("api/company/department/")]
        [HttpPost]
        public ActionResult SaveDepartment(SaveData<DEPARTMENTS_UI> saveDepartment)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, saveDepartment.UUID);
                    if (rights == null || rights.IsAdmin == false)
                        return Ok(new { success = false, login = true, message = "A security issue as occured, please login." });
                    if (saveDepartment.Key != saveDepartment.Data.DEPARTMENT_PK)
                        return Ok(new { success = false, message = "Invalid Department, keys do not match - thx" });

                    Departments department;
                    if (saveDepartment.Key == Guid.Empty)
                    {
                        department = saveDepartment.Data.GetDepartment(rights.GroupKey);
                        context.Departments.Add(department);
                    }
                    else
                    {
                        department = (from sc in context.Departments
                                where sc.DepartmentPk == saveDepartment.Key && sc.GroupFk == rights.GroupKey
                                select sc).FirstOrDefault();
                        if (department == null)
                            return Ok(new { success = false, message = "Cannot find this department - thx" });
                        saveDepartment.Data.SetToDepartment(department, rights.GroupKey);
                    }

                    context.SaveChanges();
                    return Ok(new { success = true, message = "Department saved.", key =department.DepartmentPk  });
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "save a department");
                return Ok(new { success = false, message = message });
            }
        }

        [Route("api/company/department/{departmentKey}/{uuidKey}")]
        [HttpDelete]
        public object DeleteDepartment(Guid departmentKey, string uuidKey)
        {
            LoginRights rights = null;
            try
            {
                using (inctrak_goalsContext context = new GoalsContext(_options.Value))
                {
                    rights = GetLoginUser(context, uuidKey);
                    if (rights == null || rights.IsAdmin == false)
                        return new { success = false, login = true, message = "A security issue as occured, please login." };

                    if (departmentKey != Guid.Empty)
                    {
                        Departments department = (from sc in context.Departments
                                     where sc.DepartmentPk == departmentKey && sc.GroupFk == rights.GroupKey
                                     select sc).FirstOrDefault();
                        if (department != null)
                        {
                            context.Departments.Remove(department);
                            context.SaveChanges();
                            return new { success = true, Departments = QueryDepartments(context, rights) };
                        }
                    }
                    throw new Exception("Department not found");
                }
            }
            catch (Exception excp)
            {
                string message = IncTrakErrors.LogError(_options.Value, rights ?? GetLoginUser(), excp, "remove a department");
                return new { success = false, message = message };
            }
        }
    }
}