﻿global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using mbill.Service.Core.Auth.Input;
global using mbill.Service.Core.User.Output;
global using mbill.Core.AOP.Attributes;
global using mbill.Core.Domains.Common;
global using mbill.Core.Domains.Common.Enums.Base;
global using mbill.Core.Domains.Entities.Core;
global using mbill.Core.Domains.Entities.User;
global using mbill.Core.Domains.Entities.Bill;
global using mbill.Core.Domains.Entities.PreOrder;
global using mbill.Core.Exceptions;
global using mbill.Core.Extensions;
global using mbill.Core.Interface.IRepositories.Core;
global using mbill.Core.Interface.IRepositories.PreOrder;
global using mbill.Service.Base;
global using mbill.Service.Core.User;
global using mbill.Service.Core.User.Input;
global using mbill.ToolKits.Utils;
global using mbill.Core.Domains.Common.Consts;
global using mbill.Service.Core.Auth.Output;
global using mbill.Service.Core.Wx;
global using Microsoft.Extensions.Logging;
global using mbill.Service.Bill.Bill.Output;
global using mbill.Core.Domains.Common.Enums;
global using mbill.Core.Interface.IRepositories.Bill;
global using System.Linq.Expressions;
global using AutoMapper;
global using mbill.Service.Bill.Bill.Input;
global using Microsoft.Extensions.DependencyInjection;
global using FreeSql;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.Extensions.Logging.Abstractions;
global using mbill.Core.Security;
global using mbill.Core.Domains.Common.Base;
global using mbill.Core.Interface.IRepositories.Base;
global using System.ComponentModel.DataAnnotations;
global using mbill.Service.Bill.Asset.Output;
global using mbill.Service.Bill.Asset.Input;
global using mbill.Service.Bill.Category.Output;
global using System.Data;
global using mbill.Service.Bill.Category.Input;
global using mbill.Service.Core.Logger.Output;
global using mbill.Service.Core.Permission.Input;
global using mbill.Service.Core.Permission.Output;
global using mbill.Service.Core.Logger.Input;
global using mbill.Service.Common.Common.Converter;
global using mbill.Core.Common.Configs;
global using DotNetCore.Security;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using mbill.Core.Interface.IDependency;
global using mbill.Service.Core.Files.Output;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Hosting;
global using System.IO;
global using Newtonsoft.Json;
global using mbill.Service.Core.Wx.Output;
global using System.Net.Http;
global using mbill.Service.PreOrder.Input;
global using mbill.Service.PreOrder.Output;

global using static mbill.Core.Domains.Common.Consts.SystemConst;
