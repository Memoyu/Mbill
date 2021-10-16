﻿using DotNetCore.Security;
using mbill_service.Core.Common.Configs;
using mbill_service.Core.Domains.Common;
using mbill_service.Core.Domains.Common.Consts;
using mbill_service.Core.Domains.Entities.Core;
using mbill_service.Core.Domains.Entities.User;
using mbill_service.Core.Extensions;
using mbill_service.Core.Interface.IRepositories.Core;
using mbill_service.Service.Core.Auth.Input;
using mbill_service.Service.Core.Wx.Input;
using mbill_service.Service.Core.Wx.Output;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace mbill_service.Service.Core.Wx
{
    public class WxService : IWxService
    {
        private static string WxJscode(string appid, string secret, string code) => $"https://api.weixin.qq.com/sns/jscode2session?appid={appid}&secret={secret}&js_code={code}&grant_type=authorization_code";
        private static string WxGetAccessToken(string appid, string secret) => $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appid}&secret={secret}";
        private static string WxGetUserInfo(string accessToken, string openId) => $"https://api.weixin.qq.com/cgi-bin/user/info?access_token={accessToken}&openid={openId}";

        private readonly ILogger<WxService> _logger;
        private readonly IHttpClientFactory _httpClient;
        private readonly IUserRepo _userRepo;
        private readonly IUserIdentityRepo _userIdentityRepo;
        private readonly IJsonWebTokenService _jsonWebTokenService;

        public WxService(ILogger<WxService> logger, IHttpClientFactory httpClient, IUserRepo userRepo, IUserIdentityRepo userIdentityRepo, IJsonWebTokenService jsonWebTokenService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _userRepo = userRepo; 
            _userIdentityRepo = userIdentityRepo;
            _jsonWebTokenService = jsonWebTokenService;
        }

        public async Task<ServiceResult<WxCode2SessionDto>> GetCode2Session(string code)
        {
            var url = WxJscode(Appsettings.MinPro.AppID, Appsettings.MinPro.AppSecret, code);
            using var client = _httpClient.CreateClient();//创建HttpClient请求
            var httpResponse = await client.GetAsync(url);//请求获取
            if (httpResponse.StatusCode != HttpStatusCode.OK)//判断请求响应是否成功
                return ServiceResult<WxCode2SessionDto>.Failed("请求微信Code2Session响应失败");

            var content = await httpResponse.Content.ReadAsStringAsync();//获取响应内容
            var code2Session = content.FromJson<WxCode2SessionDto>();
            if (code2Session.ErrCode != 0)
                return ServiceResult<WxCode2SessionDto>.Failed("请求微信Code2Session返回失败");
            return ServiceResult<WxCode2SessionDto>.Successed(code2Session);
        }

        public Task<ServiceResult<TokenDto>> WxLoginAsync(WxUserInfoInput input)
        {
            //var exist = await _userIdentityRepo.Ge
            //if ()
            //{

            //}

            //var user = new UserEntity();
            //user.UserRoles = new List<UserRoleEntity>
            //{
            //    new UserRoleEntity()
            //    {
            //        RoleId = SystemConst.Role.User
            //    }
            //};

            //user.UserIdentitys = new List<UserIdentityEntity>()//构建赋值用户身份认证登录信息
            //{
            //    new UserIdentityEntity(UserIdentityEntity.WeiXin,input.Nickname,input.OpenId,DateTime.Now)
            //};
            //await _userRepo.InsertAsync(user);
            throw new NotImplementedException();
        }
    }
}
