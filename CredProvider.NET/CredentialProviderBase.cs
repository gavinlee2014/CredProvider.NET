using CredProvider.NET.Interop2;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static CredProvider.NET.Constants;

namespace CredProvider.NET
{
    public abstract class CredentialProviderBase : ICredentialProvider, ICredentialProviderSetUserArray
    {
        protected ICredentialProviderEvents events;
        protected ulong adviseContext;
        


        protected abstract CredentialView Initialize(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags);

        protected CredentialView view;
        private _CREDENTIAL_PROVIDER_USAGE_SCENARIO usage;

        private List<ICredentialProviderUser> providerUsers;

        public void TriggerUIEvent()
        {
            Logger.Write("events is null?" + (events == null));
            if (events != null)
            {
                events.CredentialsChanged(adviseContext);
            }
        }

        public virtual int SetUsageScenario(_CREDENTIAL_PROVIDER_USAGE_SCENARIO cpus, uint dwFlags)
        {
            Logger.Write();
            view = Initialize(cpus, dwFlags);
            usage = cpus;

            if (view.Active)
            {
                return HRESULT.S_OK;
            }

            return HRESULT.E_NOTIMPL;
        }

        public virtual int SetSerialization(ref _CREDENTIAL_PROVIDER_CREDENTIAL_SERIALIZATION pcpcs)
        {
            Logger.Write($"ulAuthenticationPackage: {pcpcs.ulAuthenticationPackage}");

            return HRESULT.S_OK;
        }

        public virtual int Advise(ICredentialProviderEvents pcpe, ulong upAdviseContext)
        {
            Logger.Write($"upAdviseContext: {upAdviseContext}");

            if (pcpe != null)
            {
                events = pcpe;

                Marshal.AddRef(Marshal.GetIUnknownForObject(pcpe));

                adviseContext = upAdviseContext;
            }

            return HRESULT.S_OK;
        }

        public virtual int UnAdvise()
        {
            Logger.Write();

            if (events != null)
            {
                //Marshal.Release(Marshal.GetIUnknownForObject(events));
                events = null;
            }

            return HRESULT.S_OK;
        }

        public virtual int GetFieldDescriptorCount(out uint pdwCount)
        {
            Logger.Write();

            pdwCount = (uint)view.DescriptorCount;

            Logger.Write($"Returning field count: {pdwCount}");

            return HRESULT.S_OK;
        }

        public virtual int GetFieldDescriptorAt(uint dwIndex, [Out] IntPtr ppcpfd)
        {
            if (view.GetField((int)dwIndex, ppcpfd))
            {
                return HRESULT.S_OK;
            }

            return HRESULT.E_INVALIDARG;
        }

        public virtual int GetCredentialCount(
            out uint pdwCount,
            out uint pdwDefault,
            out int pbAutoLogonWithDefault
        )
        {
            Logger.Write();

            pdwCount = (uint)view.CredentialCount;

            pdwDefault = (uint)view.DefaultCredential;

            if (view.onLogin)
            {
                pbAutoLogonWithDefault = 1;
            }
            else
            {
                pbAutoLogonWithDefault = 0;
            }
            

            return HRESULT.S_OK;
        }

        public virtual int GetCredentialAt(uint dwIndex, out ICredentialProviderCredential ppcpc)
        {
            Logger.Write($"dwIndex: {dwIndex}");

            ppcpc = view.CreateCredential((int)dwIndex);

            return HRESULT.S_OK;
        }

        public virtual _CREDENTIAL_PROVIDER_USAGE_SCENARIO GetUsage()
        {
            return usage;
        }

        public virtual int SetUserArray(ICredentialProviderUserArray users)
        {
            Logger.Write();
            try
            {
                this.providerUsers = new List<ICredentialProviderUser>();

                Logger.Write("users is null? " + (users == null));
                Logger.Write("users =" + users);
                users.GetCount(out uint count);
                users.GetAccountOptions(out CREDENTIAL_PROVIDER_ACCOUNT_OPTIONS options);

                Logger.Write($"count: {count}; options: {options}");

                for (uint i = 0; i < count; i++)
                {
                    users.GetAt(i, out ICredentialProviderUser user);

                    user.GetProviderID(out Guid providerId);
                    user.GetSid(out string sid);

                    this.providerUsers.Add(user);

                    Logger.Write($"providerId: {providerId}; sid: {sid}");
                }

                return HRESULT.S_OK;
            }
            catch (Exception e)
            {
                Logger.Write("SetUserArray err:" + e.ToString() + "\r\n" + e.StackTrace);
                return HRESULT.E_UNEXPECTED;
            }
        }

        //Lookup the user by index and return the sid
        public virtual string GetUserSid(int dwIndex)
        {
            Logger.Write();

            Logger.Write("providerUsers is null? " + (providerUsers == null));
            Logger.Write("providerUsers =" + providerUsers);

            //CredUI does not provide user sids, so return null
            if (this.providerUsers.Count < dwIndex + 1) return null;
            Logger.Write("providerUsers at index " + dwIndex + " " + providerUsers[dwIndex]);
            this.providerUsers[dwIndex].GetSid(out string sid);
            return sid;
        }
    }
}
