using Common.Authority.Bo;
using Common.Authority.Dto;
using Common.Authority.Entity;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Common.Authority.Core
{
    /**********************************************************************
	*创 建 人： 
	*创建时间：2019/10/22 10:18:57
	*描  述  ：
	***********************************************************************/
    public class AuthorityService
    {
        /// <summary>
        /// 初始化模块
        /// </summary>
        public void Initialize()
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                session.SetBatchSize(200);

                using (ITransaction transaction = session.BeginTransaction())
                {
                    var modules = ReadJsonData();

                    foreach (var item in modules)
                    {
                        if (!item.IsCommon)
                        {
                            AuthorityModule authorityModule = new AuthorityModule
                            {
                                Name = item.Name,
                                Icon = item.Icon,
                                Enable = true
                            };

                            InitAuthorityPage(ref authorityModule, item.Children);

                            session.SaveOrUpdate(authorityModule);
                        }
                    }

                    transaction.Commit();
                }
                
            }
        }
        /// <summary>
        /// 初始化模块页面
        /// </summary>
        /// <param name="session"></param>
        /// <param name="moduleName"></param>
        /// <param name="moduleId"></param>
        private void InitAuthorityPage(ref AuthorityModule authorityModule, IList<AuthorityBo> pages)
        {
            if (pages.Any())
            {
                foreach (var item in pages)
                {
                    AuthorityPage authorityPage = new AuthorityPage
                    {
                        Name = item.Name,
                        AuthorityModule = authorityModule,
                        Enable = true
                    };

                    InitPermission(ref authorityPage, item.Children);

                    authorityModule.AuthorityPages.Add(authorityPage);
                }
            }
        }
        /// <summary>
        /// 初始化权限
        /// </summary>
        /// <param name="session"></param>
        /// <param name="pageName"></param>
        /// <param name="pageId"></param>
        private void InitPermission(ref AuthorityPage authorityPage, IList<AuthorityBo> pagePermissions)
        {
            if (pagePermissions.Any())
            {
                foreach (var item in pagePermissions)
                {
                    Permission permission = new Permission
                    {
                        Name = item.Name,
                        Value = Convert.ToInt32(item.Value),
                        AuthorityPage = authorityPage,
                        Enable = true
                    };

                    authorityPage.Permissions.Add(permission);
                }
            }
        }
        /// <summary>
        /// 读取Json数据并序列化
        /// </summary>
        /// <returns></returns>
        private IList<AuthorityBo> ReadJsonData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetName().Name + ".actionAuth.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
                {
                    string jsonData = reader.ReadToEnd();

                    Rootobject rootobject = JsonConvert.DeserializeObject<Rootobject>(jsonData);

                    return rootobject.Modules;
                }
            }
        }
        /// <summary>
        /// 查询角色授权模块
        /// </summary>
        /// <returns></returns>
        public RoleModuleDto QueryRoleEnableAuthorityModule(int roleId)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                Role role = session
                    .QueryOver<Role>()
                    .And(x => x.Id == roleId)
                    .Fetch()
                    .Left
                    .JoinQueryOver<RoleModule>(x=>x.RoleModules)
                    .Fetch()
                    .Left
                    .JoinQueryOver(x=>x.AuthorityModule)
                    .List()
                    .FirstOrDefault();

                IList<AuthorityModule> authorityModules = session
                    .QueryOver<AuthorityModule>()
                    .And(x => x.Enable)
                    .List();

                if (role != null)
                {
                    IList<AuthorityModule> roleModules = role.RoleModules.Select(x => x.AuthorityModule).ToList();

                    IList<AuthorityModule> intersectModules = roleModules.Intersect(authorityModules).Distinct().ToList();

                    foreach (var item in authorityModules)
                    {
                        if (intersectModules.Contains(item))
                        {
                            item.Checked = true;
                        }
                        else
                        {
                            item.Checked = false;
                        }
                    }

                    RoleModuleDto roleModuleDto = new RoleModuleDto
                    {
                        RoleDto = AutoMapper.Mapper.Map<RoleDto>(role),
                        AuthorityModuleDtos = AutoMapper.Mapper.Map<IList<AuthorityModuleDto>>(authorityModules)
                    };

                    return roleModuleDto;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 职务模块授权
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="moduleIds"></param>
        public void UpdateRoleModule(int roleId, int[] moduleIds)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    Role role = session
                        .QueryOver<Role>()
                        .And(x => x.Id == roleId)
                        .List()
                        .FirstOrDefault();

                    if (role.RoleModules.Any())
                    {
                        role.RoleModules.Clear();
                    }

                    if (role.RolePermissions.Any())
                    {
                        role.RolePermissions.Clear();
                    }

                    IList<AuthorityModule> authorityModules = session
                        .QueryOver<AuthorityModule>()
                        .And(Restrictions.In("Id", moduleIds))
                        .And(x => x.Enable)
                        .List();

                    foreach (var item in authorityModules)
                    {
                        RoleModule roleModule = new RoleModule
                        {
                            Role = role,
                            AuthorityModule = item
                        };

                        if (item.AuthorityPages.Any())
                        {
                            foreach (var page in item.AuthorityPages)
                            {
                                foreach (var permission in page.Permissions)
                                {
                                    RolePermission rolePermission = new RolePermission
                                    {
                                        Permission = permission,
                                        AuthorityPage = page,
                                        Role = role
                                    };
                                    role.RolePermissions.Add(rolePermission);
                                }
                            }
                        }
                        role.RoleModules.Add(roleModule);
                    }

                    session.SaveOrUpdate(role);

                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 查询角色权限树
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IList<PermissionTreeDto> QueryRolePermissionForTree(int roleId)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    List<PermissionTreeDto> trees = new List<PermissionTreeDto>();

                    PermissionTreeDto tree = new PermissionTreeDto
                    {
                        Title = "系统权限",
                        Expand = true,
                        Category = "root",
                        Id = 0,
                        Children = new List<PermissionTreeDto>()
                    };

                    Role role = session
                       .QueryOver<Role>()
                       .And(x => x.Id == roleId)
                       .Fetch(SelectMode.ChildFetch, x => x.RoleModules)
                       .Fetch(SelectMode.ChildFetch, x => x.RolePermissions)
                       .List()
                       .FirstOrDefault();

                    if (role.RoleModules.Any())
                    {
                        foreach (var module in role.RoleModules)
                        {
                            PermissionTreeDto childModule = new PermissionTreeDto
                            {
                                Title = module.AuthorityModule.Name,
                                Expand = false,
                                Id = module.AuthorityModule.Id,
                                Category = "module",
                                Children = new List<PermissionTreeDto>()
                            };

                            AuthorityModule authorityModule = module.AuthorityModule;

                            if (authorityModule.AuthorityPages.Any())
                            {
                                foreach (var page in authorityModule.AuthorityPages)
                                {
                                    PermissionTreeDto childPage = new PermissionTreeDto
                                    {
                                        Title = page.Name,
                                        Expand = true,
                                        Id = page.Id,
                                        Category = "page",
                                        Children = new List<PermissionTreeDto>()
                                    };


                                    if (page.Permissions.Any())
                                    {
                                        var rolePermissions = role.RolePermissions.Select(x => x.Permission);

                                        foreach (var permission in page.Permissions)
                                        {
                                            if (page.Id == permission.AuthorityPage.Id)
                                            {
                                                bool isChecked = rolePermissions.Contains(permission);

                                                PermissionTreeDto childPermission = new PermissionTreeDto
                                                {
                                                    Title = permission.Name,
                                                    Expand = true,
                                                    Id = permission.Id,
                                                    Category = "per",
                                                    Checked = isChecked,
                                                    Children = new List<PermissionTreeDto>()
                                                };
                                                childPage.Children.Add(childPermission);
                                            }
                                        }
                                    }
                                    childModule.Children.Add(childPage);
                                }
                            }
                            tree.Children.Add(childModule);
                        }
                    }

                    trees.Add(tree);

                    transaction.Commit();

                    return trees;
                }
            }
        }
        /// <summary>
        /// 更新角色权限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissionIds"></param>
        public void UpdateRolePermission(int roleId, int[] permissionIds)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    Role role = session
                        .QueryOver<Role>()
                        .And(x => x.Id == roleId)
                        .List()
                        .FirstOrDefault();

                    if (role != null)
                    {
                        if (role.RolePermissions.Any())
                        {
                            role.RolePermissions.Clear();
                        }

                        IList<Permission> permissions = session
                            .QueryOver<Permission>()
                            .And(Restrictions.In("Id", permissionIds))
                            .Fetch(SelectMode.Fetch, x => x.AuthorityPage)
                            .JoinQueryOver<AuthorityPage>(x => x.AuthorityPage)
                            .List();

                        foreach (var item in permissions)
                        {
                            RolePermission rolePermission = new RolePermission
                            {
                                AuthorityPage = item.AuthorityPage,
                                Permission = item,
                                Role = role
                            };
                            role.RolePermissions.Add(rolePermission);
                        }

                        session.SaveOrUpdate(role);
                    }

                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 构建角色Vue动态路由
        /// </summary>
        /// <param name="roleId">角色id</param>
        /// <param name="isSuper">是否是系统管理员</param>
        public IList<VueRouterDto> GenerateVueRouter(int roleId, bool isSuper)
        {
            var roleMenu = GenerateVueMenu(roleId, isSuper);

            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetName().Name + ".routerData.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("UTF-8")))
                {
                    string jsonData = reader.ReadToEnd();

                    VueRouterRootDto rootobject = JsonConvert.DeserializeObject<VueRouterRootDto>(jsonData);

                    IList<VueRouterDto> routerDtos = rootobject.Routers;
                    if (!isSuper)
                    {
                        LoopCompareUserRouter(ref routerDtos, roleMenu);
                    }
                    return routerDtos;
                }
            }

        }
        /// <summary>
        /// 对比角色授权模块构建路由数据
        /// </summary>
        /// <param name="vueRouters"></param>
        /// <param name="vueMenus"></param>
        private void LoopCompareUserRouter(ref IList<VueRouterDto> vueRouters, IList<VueMenuDto> vueMenus)
        {
            var roleModule = vueMenus.Select(x => x.Name).ToList();

            for (int i = vueRouters.Count - 1; i >= 0; i--)
            {
                var router = vueRouters[i];

                if (router.Children.Any())
                {
                    for (int m = router.Children.Count - 1; m >= 1; m--)
                    {
                        var child = router.Children[m];
                        if (!child.IsPage) continue;
                        //判断角色是否包含模块路由，如果不包含则移除
                        if (!roleModule.Contains(child.Alias))
                        {
                            router.Children.Remove(child);
                        }
                        else
                        {
                            var rolePages = vueMenus.FirstOrDefault(x => x.Name == child.Alias).Pages.Select(x => x.Name).ToList();
                            //合并不需要验证的页面
                            rolePages = rolePages.Union(child.Children.Where(x => x.NotAuth).Select(x => x.Alias).ToList()).ToList();
                            //判断角色是否包含模块页面，如果不包含则移除
                            var pages = child.Children;
                            for (int p = pages.Count - 1; p >= 0; p--)
                            {
                                var page = pages[p];

                                if (!rolePages.Contains(page.Alias) && page.IsPage)
                                {
                                    child.Children.Remove(page);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 构建角色Vue动态菜单
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IList<VueMenuDto> GenerateVueMenu(int roleId, bool isSuper)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    IList<VueMenuDto> vueMenuDtos = new List<VueMenuDto>();

                    if (!isSuper)
                    {
                        RolePermission rolePermission = null;

                        Permission permission = null;

                        Role role = session
                            .QueryOver<Role>()
                            .And(x => x.Id == roleId)
                            .JoinQueryOver(x => x.RolePermissions, () => rolePermission)
                            .JoinAlias(x => x.Permission, () => permission)
                            .JoinQueryOver(x => x.AuthorityPage)
                            .List()
                            .FirstOrDefault();

                        if (role != null)
                        {
                            //构建授权模块
                            foreach (var item in role.RoleModules)
                            {
                                var module = item.AuthorityModule;

                                if (role.RolePermissions.Select(x => x.AuthorityPage).Any(x => x.AuthorityModule.Name == module.Name))
                                {
                                    VueMenuDto menuDto = new VueMenuDto
                                    {
                                        Name = module.Name,
                                        Icon = module.Icon
                                    };

                                    var pages = role.RolePermissions.Select(x => x.AuthorityPage).Where(x => x.AuthorityModule.Name == module.Name).Distinct();
                                    //合并不需要验证的页面
                                    pages = pages.Union(module.AuthorityPages.Where(x => x.NotAuth).ToList());

                                    foreach (var page in pages)
                                    {
                                        if (page.Enable)
                                        {
                                            VueMenuPageDto pageDto = new VueMenuPageDto
                                            {
                                                Name = page.Name,
                                                Permission = role.RolePermissions.Where(x => x.AuthorityPage.Name == page.Name).Select(x => x.Permission.Value).Sum()
                                            };

                                            menuDto.Pages.Add(pageDto);
                                        }
                                    }

                                    vueMenuDtos.Add(menuDto);
                                }
                            }
                        }
                    }
                    else
                    {
                        IList<AuthorityModule> modules = session
                            .QueryOver<AuthorityModule>()
                            .And(x => x.Enable)
                            .List();

                        foreach (var module in modules)
                        {
                            VueMenuDto menuDto = new VueMenuDto
                            {
                                Name = module.Name,
                                Icon = module.Icon
                            };

                            foreach (var page in module.AuthorityPages)
                            {
                                if (page.Enable)
                                {
                                    VueMenuPageDto pageDto = new VueMenuPageDto
                                    {
                                        Name = page.Name,
                                        Permission = page.Permissions.Select(x => x.Value).Sum()
                                    };

                                    menuDto.Pages.Add(pageDto);
                                }
                            }

                            vueMenuDtos.Add(menuDto);

                        }
                    }
                    transaction.Commit();

                    return vueMenuDtos;
                }
            }
        }
        /// <summary>
        /// 更新角色授权门店数据范围
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="stores"></param>
        public void UpdateRoleStores(int roleId, int[] stores)
        {
            if (!stores.Any())
            {
                throw new Exception("授权数据不能为空！");
            }
            if (roleId <= 0)
            {
                throw new Exception("非法的角色！");
            }
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.SetBatchSize(200);
                    //删除历史记录
                    string hql = "DELETE FROM RoleStores WHERE RoleId = :p1";
                    session.CreateQuery(hql)
                        .SetParameter("p1", roleId)
                        .ExecuteUpdate();

                    foreach (var item in stores)
                    {
                        RoleStores roleStores = new RoleStores
                        {
                            RoleId = roleId,
                            StoreId = item
                        };
                        session.SaveOrUpdate(roleStores);
                    }

                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// 查询角色授权门店
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public IList<int> QueryRoleStores(int roleId)
        {
            using (ISession session = AuthoritySessionProvider.GetSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    var result = session.QueryOver<RoleStores>()
                        .And(x => x.RoleId == roleId)
                        .List()
                        .Select(x=>x.StoreId)
                        .ToList();
                    transaction.Commit();
                    return result;
                }
            }
        }
    }
}
