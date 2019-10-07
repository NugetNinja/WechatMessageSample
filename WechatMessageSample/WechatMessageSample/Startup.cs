using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.NeuChar.App.AppStore;
using Senparc.NeuChar.Entities;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.Weixin.MP.MessageHandlers.Middleware;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.Work;
using Senparc.Weixin.WxOpen;

namespace WechatMessageSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();//ʹ�ñ��ػ���������

            services.AddSenparcWeixinServices(Configuration);//Senparc.Weixin ע��
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            //����EnableRequestRewind�м��
            app.UseEnableRequestRewind();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister => { })
               //ʹ�� Senparc.Weixin SDK
               .UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
               {
                   #region ע�ṫ�ںŻ�С���򣨰��裩

                   //ע�ṫ�ںţ���ע������
                   weixinRegister
                          .RegisterMpAccount(senparcWeixinSetting.Value, "��ʢ������С���֡����ں�")
                          //ע�������ںŻ�С���򣨿�ע������
                          .RegisterWxOpenAccount(senparcWeixinSetting.Value, "��ʢ������С���֡�С����")
                   #endregion

                   #region ע����ҵ�ţ����裩

                          //ע����ҵ΢�ţ���ע������
                          .RegisterWorkAccount(senparcWeixinSetting.Value, "��ʢ�����硿��ҵ΢��");
                   #endregion
               });

            //ʹ�� ���ں� MessageHandler �м��
            app.UseMessageHandlerForMp("/Weixin", CustomMessageHandler.GenerateMessageHandler,
                o => o.AccountSettingFunc = c => senparcWeixinSetting.Value);
        }
    }

    /// <summary>
    /// ����չʾ���㣬д��ͬһ���ļ��У�ʵ�ʿ���������뵽�����ļ�
    /// </summary>
    public class CustomMessageHandler : Senparc.Weixin.MP.MessageHandlers.MessageHandler<DefaultMpMessageContext>
    {
        /// <summary>
        /// Ϊ�м���ṩ���ɵ�ǰ���ί��
        /// </summary>
        public static Func<Stream, PostModel, int, CustomMessageHandler> GenerateMessageHandler = (stream, postModel, maxRecordCount)
                        => new CustomMessageHandler(stream, postModel, maxRecordCount, false/* �Ƿ�ֻ�����������Ϣ������߰�ȫ�� */);


        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEcryptMessage = false, DeveloperInfo developerInfo = null)
            : base(inputStream, postModel, maxRecordCount, onlyAllowEcryptMessage, developerInfo)
        {
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            var responseMessage = base.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "����һ��Ĭ����Ϣ";
            return responseMessage;
        }
    }
}
