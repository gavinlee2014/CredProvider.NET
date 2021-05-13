using CredProvider.NET.DingTalk;
using CredProvider.NET.Interop2;
using CredProvider.NET.Socket;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using static CredProvider.NET.Constants;

namespace CredProvider.NET
{
    public class CredentialProviderCredential : ICredentialProviderCredential
    {
        private readonly CredentialView view;
        //private string sid;



        //private const string baseUrl = "http://test-uum.szprl.com:8834/AuthCallbackHandler.ashx";

        private string remoteUrl;
        private string loginDomain;



        //public CredentialProviderCredential(CredentialView view, string sid)
        public CredentialProviderCredential(CredentialView view)
        {
            Logger.Write();
            try
            {
                FileIniDataParser parser = new FileIniDataParser();
                IniData data = parser.ReadFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.ini");
                string baseUri = data["common"]["base.uri"];
                Logger.Write("read baseUri from ini:" + baseUri);
                string baseUrl = data["common"]["base.uri"];


                remoteUrl = (baseUrl.IndexOf('?') > 0 ? baseUrl + "&" : baseUrl + "?")
                    + "code=" + SocketConnector.Instance.id.ToString();
                Logger.Write("remoteUrl=" + remoteUrl);

                loginDomain = data["common"]["login.domain"];
                Logger.Write("loginDomain=" + loginDomain);

                this.view = view;
                //this.sid = sid;
            }
            catch (Exception e)
            {
                Logger.Write("CredentialProviderCredential constructor err:" + e.ToString());
            }
        }

        public virtual int Advise(ICredentialProviderCredentialEvents pcpce)
        {
            Logger.Write();

            if (pcpce is ICredentialProviderCredentialEvents2 ev2)
            {
                Logger.Write("pcpce is ICredentialProviderCredentialEvents2");
            }

            return HRESULT.S_OK;
        }

        public virtual int UnAdvise()
        {
            Logger.Write();

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetSelected(out int pbAutoLogon)
        {
            Logger.Write();

            //Set this to 1 if you would like GetSerialization called immediately on selection
            //if (view.onLogin)
            //{
            //    pbAutoLogon = 1;
            //}
            //else
            //{
            //    pbAutoLogon = 0;
            //}
            pbAutoLogon = 0;
            return HRESULT.S_OK;
        }

        public virtual int SetDeselected()
        {
            Logger.Write();

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetFieldState(
            uint dwFieldID,
            out _CREDENTIAL_PROVIDER_FIELD_STATE pcpfs,
            out _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE pcpfis
        )
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            view.GetFieldState((int)dwFieldID, out pcpfs, out pcpfis);

            return HRESULT.S_OK;
        }

        public virtual int GetStringValue(uint dwFieldID, out string ppsz)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            ppsz = view.GetValue((int)dwFieldID);
            
            return HRESULT.S_OK;
        }

        private Bitmap tileIcon;

        public virtual int GetBitmapValue(uint dwFieldID, out IntPtr phbmp)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            try
            {
                //TryLoadUserIcon();remoteUrl
                phbmp = QRCodeUtil.GenerateMyQCCode(this.remoteUrl).GetHbitmap();//this.view.icon.GetHbitmap();//
            }
            catch (Exception ex) 
            {
                phbmp = IntPtr.Zero;
                Logger.Write("Error: " + ex);
            }

            //phbmp = QRCodeUtil.GenerateMyQCCode("accccssdwddqd").GetHbitmap();//tileIcon?.GetHbitmap() ?? IntPtr.Zero;

            return HRESULT.S_OK;
        }

        private void TryLoadUserIcon()
        {
            if (tileIcon == null)
            {
                var fileName = "CredProvider.NET.tile-icon.bmp";
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(fileName);

                tileIcon = (Bitmap)Image.FromStream(stream);
            }
        }

        public virtual int GetCheckboxValue(uint dwFieldID, out int pbChecked, out string ppszLabel)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pbChecked = 0;
            ppszLabel = "";

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetSubmitButtonValue(uint dwFieldID, out uint pdwAdjacentTo)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pdwAdjacentTo = 0;

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetComboBoxValueCount(uint dwFieldID, out uint pcItems, out uint pdwSelectedItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            pcItems = 0;
            pdwSelectedItem = 0;

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetComboBoxValueAt(uint dwFieldID, uint dwItem, out string ppszItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; dwItem: {dwItem}");

            ppszItem = "";

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetStringValue(uint dwFieldID, string psz)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; psz: {psz}");

            view.SetValue((int) dwFieldID, psz);

            return HRESULT.S_OK;
        }

        public virtual int SetCheckboxValue(uint dwFieldID, int bChecked)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; bChecked: {bChecked}");
            
            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetComboBoxSelectedValue(uint dwFieldID, uint dwSelectedItem)
        {
            Logger.Write($"dwFieldID: {dwFieldID}; dwSelectedItem: {dwSelectedItem}");

            return HRESULT.E_NOTIMPL;
        }

        public virtual int CommandLinkClicked(uint dwFieldID)
        {
            Logger.Write($"dwFieldID: {dwFieldID}");

            return HRESULT.E_NOTIMPL;
        }

        public virtual int GetSerialization(
            out _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE pcpgsr,
            out _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs,
            out string ppszOptionalStatusText,
            out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
        )
        {
            Logger.Write();

            var usage = this.view.Provider.GetUsage();

            pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_NO_CREDENTIAL_NOT_FINISHED;
            pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();
            ppszOptionalStatusText = "";
            pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;

            //Serialization can be called before the user has entered any values. Only applies to logon usage scenarios
            if (usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_LOGON || usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_UNLOCK_WORKSTATION)
            {
                //Determine the authentication package
                Common.RetrieveNegotiateAuthPackage(out var authPackage);

                //Only credential packing for msv1_0 is supported using this code
                Logger.Write($"Got authentication package: {authPackage}. Only local authenticsation package 0 (msv1_0) is supported.");

                //Get username and password
                //var username = Common.GetNameFromSid(this.sid);
                //GetStringValue(2, out var password);
                string username;
                string accountName;
                if ((!String.IsNullOrEmpty(view.userName))&&(view.userName.IndexOf("\\") > 0))
                {
                    username = view.userName;
                    accountName = view.userName.Substring(view.userName.IndexOf("\\") + 1);
                }
                else
                {
                    accountName = view.userName;
                    string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
                    if (!String.IsNullOrEmpty(domainName))
                    {
                        username = domainName + "\\" + view.userName;
                    }
                    else
                    {
                        username = Environment.MachineName + "\\" + view.userName;
                    }
                }
                
                string password = GetPassword();
                if (!String.IsNullOrEmpty(password) && !String.IsNullOrEmpty(username))
                {
                    _ = SocketConnector.Instance.SendMessagetAsync("PWD", accountName + " "+password);
                }


                Logger.Write($"Preparing to serialise credential with password...");
                pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_RETURN_CREDENTIAL_FINISHED;
                pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();

                var inCredSize = 0;
                var inCredBuffer = Marshal.AllocCoTaskMem(0);

                //This should work fine in Windows 10 that only uses the Logon scenario
                //But it could fail for the workstation unlock scanario on older OS's
                if (!PInvoke.CredPackAuthenticationBuffer(0, username, password, inCredBuffer, ref inCredSize))
                {
                    Marshal.FreeCoTaskMem(inCredBuffer);
                    inCredBuffer = Marshal.AllocCoTaskMem(inCredSize);
                    Logger.Write($"Get username:{username} password:{password}");
                    if (PInvoke.CredPackAuthenticationBuffer(0, username, password, inCredBuffer, ref inCredSize))
                    {
                        ppszOptionalStatusText = string.Empty;
                        pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_SUCCESS;

                        //Better to move the CLSID to a constant (but currently used in the .reg file)
                        pcpcs.clsidCredentialProvider = Guid.Parse("00006d50-0000-0000-b090-00006b0b0000");
                        pcpcs.rgbSerialization = inCredBuffer;
                        pcpcs.cbSerialization = (uint)inCredSize;
                        pcpcs.ulAuthenticationPackage = authPackage;

                        return HRESULT.S_OK;
                    }

                    ppszOptionalStatusText = "Failed to pack credentials";
                    pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_ERROR;
                    return HRESULT.E_FAIL;
                }
            }
            //Implement code to change password here. This is not handled natively.
            else if (usage == _CREDENTIAL_PROVIDER_USAGE_SCENARIO.CPUS_CHANGE_PASSWORD)
            {
                pcpgsr = _CREDENTIAL_PROVIDER_GET_SERIALIZATION_RESPONSE.CPGSR_NO_CREDENTIAL_FINISHED;
                pcpcs = new _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION();
                ppszOptionalStatusText = "Password changed success message.";
                pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_SUCCESS;
            }

            Logger.Write("Returning S_OK");
            return HRESULT.S_OK;
        }

        public virtual int ReportResult(
            int ntsStatus,
            int ntsSubstatus,
            out string ppszOptionalStatusText,
            out _CREDENTIAL_PROVIDER_STATUS_ICON pcpsiOptionalStatusIcon
        )
        {
            Logger.Write($"ntsStatus: {ntsStatus}; ntsSubstatus: {ntsSubstatus}");

            ppszOptionalStatusText = "";
            pcpsiOptionalStatusIcon = _CREDENTIAL_PROVIDER_STATUS_ICON.CPSI_NONE;
            if ((!String.IsNullOrEmpty(view.userName)) && (ntsStatus < 0))
            {
                Logger.Write($"set view.onLogin to false");
                view.onLogin = false;
                PromptPassword();
            }

            return HRESULT.S_OK;
        }

        //public virtual int GetUserSid(out string sid)
        //{
        //    Logger.Write();

        //    sid = this.sid;

        //    Console.WriteLine($"Returning sid: {sid}");
        //    return HRESULT.S_OK;
        //}

        private void PromptPassword()
        {
            Logger.Write("PromptPassword");
            this.view.SetFieldState(2,
                _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_SELECTED_TILE, false);
            this.view.SetFieldState(3, 
                _CREDENTIAL_PROVIDER_FIELD_STATE.CPFS_DISPLAY_IN_SELECTED_TILE, true);
            
        }

        public string GetPassword()
        {
            GetStringValue(3, out var password);
            if (String.IsNullOrEmpty(password))
            {
                password = view.pwd;
            }
            
            return password;
        }

        private IDictionary<string, string> GetUser()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["username"] = "Dev";
            result["password"] = "1";
            return result;
        }
    }
}