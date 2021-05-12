using CredProvider.NET.Interop2;
using CredProvider.NET.Socket;
using Indigox.Common.Logging;
using SuperSocket.ClientEngine;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace CredProvider.NET
{
    [ComVisible(true)]
    [Guid("00006d50-0000-0000-b090-00006b0b0000")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("CredProvider.NET")]
    public class CredentialProvider : CredentialProviderBase
    {
        public static CredentialView NotActive;


        public CredentialProvider()
        {
            Logger.Write("CredentialProvider constructor");
            //id = Guid.NewGuid();
            SocketConnector.Instance.OnReceiveAuthCommand += Instance_OnReceiveAuthCommand;
            SocketConnector.Instance.OnReceiveCodeCommand += Instance_OnReceiveCodeCommand;
        }

        private void Instance_OnReceiveCodeCommand(Bitmap imageMessage)
        {
            if (view != null)
            {
                view.icon = imageMessage;
            }
            
            Logger.Write("events is null?" + (events == null));
            //if (events != null)
            //{
            //    events.CredentialsChanged(adviseContext);
            //}
        }

        private void Instance_OnReceiveAuthCommand(string accountName, string pwd)
        {
            view.SetValue(1, accountName);
            view.userName = accountName;
            view.pwd = pwd;
            Logger.Write("events is null?" + (events == null));
            TriggerUIEvent();
        }

        protected override CredentialView Initialize(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags)
        {
            Logger.Write();
            var flags = (CredentialFlag)dwFlags;
            try
            {
                Log.Debug("Initialize");
            }
            catch (Exception e)
            {
                string stack = e.ToString();
                Logger.Write($"Log.Debug err {stack}");
            }

            Logger.Write($"cpus: {cpus}; dwFlags: {flags}");

            try
            {
                var isSupported = IsSupportedScenario(cpus);

                if (!isSupported)
                {
                    if (NotActive == null) NotActive = new CredentialView(this) { Active = false };
                    return NotActive;
                }
                _ = SocketConnector.Instance.LoginToSocketAsync();

                var view = new CredentialView(this) { Active = true };
                //view.tempGuid = id;
                var userNameState = (cpus == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CREDUI) ?
                        _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_SELECTED_TILE : _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_HIDDEN;
                var confirmPasswordState = (cpus == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CHANGE_PASSWORD) ?
                        _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH : _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_HIDDEN;

                view.AddField(
                    cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_TILE_IMAGE,
                    pszLabel: "扫码登录",
                    state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_BOTH,
                    guidFieldType: Guid.Parse(CredentialView.CPFG_CREDENTIAL_PROVIDER_LOGO)
                );

                view.AddField(
                    cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_LARGE_TEXT,
                    pszLabel: "请扫码登录",
                    defaultValue: "请扫码登录",
                    state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_SELECTED_TILE
                );

                view.AddField(
                   cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_LARGE_TEXT,
                   pszLabel: "密码错误，请重新输入",
                   defaultValue: "密码错误，请重新输入",
                   state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_HIDDEN
               );

                view.AddField(
                    cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_PASSWORD_TEXT,
                    pszLabel: "请重新输入密码",
                    state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_HIDDEN,
                    guidFieldType: Guid.Parse(CredentialView.CPFG_LOGON_PASSWORD_GUID)
                );

                //view.AddField(
                //    cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_PASSWORD_TEXT,
                //    pszLabel: "Confirm password",
                //    state: confirmPasswordState,
                //    guidFieldType: Guid.Parse(CredentialView.CPFG_LOGON_PASSWORD_GUID)
                //);

                view.AddField(
                    cpft: _CREDENTIAL_PROVIDER_FIELD_TYPE.CPFT_LARGE_TEXT,
                    pszLabel: "扫码登录",
                    defaultValue: "扫码登录",
                    state: _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_DESELECTED_TILE
                );
                return view;
            }
            catch (Exception e)
            {
                Logger.Write("init err:" + e.Message + "\r\n" + e.StackTrace);
                return new CredentialView(this) { Active = true }; ;
            }
            

        }

        private static bool IsSupportedScenario(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus)
        {
            switch (cpus)
            {
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CREDUI:
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_UNLOCK_WORKSTATION:
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_LOGON:
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CHANGE_PASSWORD:
                    return true;
                
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_PLAP:
                case _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_INVALID:
                default:
                    return false;
            }
        }
    }
}
