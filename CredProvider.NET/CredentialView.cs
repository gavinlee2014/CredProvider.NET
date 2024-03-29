﻿using CredProvider.NET.Interop2;
using CredProvider.NET.Socket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CredProvider.NET
{
    public class CredentialDescriptor
    {
        public _CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR Descriptor { get; set; }

        public _CREDENTIAL_PROVIDER_FIELD_STATE State { get; set; }

        public object Value { get; set; }
    }

    public class CredentialView
    {
        private readonly List<CredentialDescriptor> fields
            = new List<CredentialDescriptor>();

        public CredentialProviderBase Provider { get; private set; }
        public bool onLogin = true;

        public const string CPFG_LOGON_PASSWORD_GUID = "60624cfa-a477-47b1-8a8e-3a4a19981827";
        public const string CPFG_CREDENTIAL_PROVIDER_LOGO = "2d837775-f6cd-464e-a745-482fd0b47493";
        public const string CPFG_CREDENTIAL_PROVIDER_LABEL = "286bbff3-bad4-438f-b007-79b7267c3d48";

        public bool Active { get; set; }

        public int DescriptorCount { get { return fields.Count; } }

        public virtual int CredentialCount { get { return 1; } }

        public virtual int DefaultCredential { get { return 0; } }

        public string userName;
        public string pwd;
        public Bitmap icon;
        //public Guid tempGuid;


        public CredentialView(CredentialProviderBase provider) 
        {
            Provider = provider;
        }

        public virtual void AddField(
            _CREDENTIAL_PROVIDER_FIELD_TYPE cpft,
            string pszLabel,
            _CREDENTIAL_PROVIDER_FIELD_STATE state,
            string defaultValue = null,
            Guid guidFieldType = default(Guid)
        )
        {
            if (!Active)
            {
                throw new NotSupportedException();
            }

            fields.Add(new CredentialDescriptor
            {
                State = state,
                Value = defaultValue,
                Descriptor = new _CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR
                {
                    dwFieldID = (uint)fields.Count,
                    cpft = cpft,
                    pszLabel = pszLabel,
                    guidFieldType = guidFieldType
                }
            });
        }

        public virtual bool GetField(int dwIndex, [Out] IntPtr ppcpfd)
        {
            Logger.Write($"dwIndex: {dwIndex}; descriptors: {fields.Count}");

            if (dwIndex >= fields.Count)
            {
                return false;
            }

            var field = fields[dwIndex];

            var pcpfd = Marshal.AllocHGlobal(Marshal.SizeOf(field.Descriptor));

            Marshal.StructureToPtr(field.Descriptor, pcpfd, false);
            Marshal.StructureToPtr(pcpfd, ppcpfd, false);

            return true;
        }

        public string GetValue(int dwFieldId)
        {
            return (string)fields[dwFieldId].Value;
        }

        public void SetValue(int dwFieldId, string val)
        {
            fields[dwFieldId].Value = val;
        }

        public void GetFieldState(
            int dwFieldId,
            out _CREDENTIAL_PROVIDER_FIELD_STATE pcpfs,
            out _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE pcpfis
        )
        {
            Logger.Write();

            var field = fields[dwFieldId];

            Logger.Write($"Returning field state: {field.State}, interactiveState: None");

            pcpfs = field.State;
            pcpfis = _CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE.CPFIS_NONE;
        }

        public void SetFieldState(
            int dwFieldId,
            _CREDENTIAL_PROVIDER_FIELD_STATE state,
            bool triggerEvent
        )
        {
            Logger.Write();
            try
            {
                var field = fields[dwFieldId];
                field.State = state;
                Logger.Write($"after SetFieldState: dwFieldId:{dwFieldId},state:{state},triggerEvent:{triggerEvent},{field.State}");
                if (triggerEvent)
                {
                    Provider.TriggerUIEvent();
                }

            }
            catch (Exception e)
            {
                Logger.Write($"get err:{e.ToString()}");
            }
        }

        private readonly Dictionary<int, ICredentialProviderCredential> credentials
            = new Dictionary<int, ICredentialProviderCredential>();

        public virtual ICredentialProviderCredential CreateCredential(int dwIndex)
        {
            Logger.Write();
            try
            {
                if (credentials.TryGetValue(dwIndex, out ICredentialProviderCredential credential))
                {
                    Logger.Write("Returning existing credential.");
                    return credential;
                }

                //Get the sid for this credential from the index
                //var sid = this.Provider.GetUserSid(dwIndex);

                credential = new CredentialProviderCredential(this);

                credentials[dwIndex] = credential;

                Logger.Write("Returning new credential.");
                return credential;
            }
            catch (Exception e)
            {
                Logger.Write("CreateCredential err:" + e.ToString() + "\r\n" + e.StackTrace);
                //var sid = this.Provider.GetUserSid(dwIndex);
                return new CredentialProviderCredential(this);
            }
        }
    }
}
