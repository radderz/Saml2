﻿using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.Metadata;
using Kentor.AuthServices.WebSso;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.Globalization;
#if NET45
using System.IdentityModel.Metadata;
#endif
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kentor.AuthServices.TestHelpers
{
    public class StubFactory
    {
        public static AuthServicesUrls CreateAuthServicesUrls()
        {
            return new AuthServicesUrls(new Uri("http://localhost"), "/AuthServices");
        }

        public static AuthServicesUrls CreateAuthServicesUrlsPublicOrigin(Uri publicOrigin)
        {
            return new AuthServicesUrls(publicOrigin, "/AuthServices");
        }

        public static SPOptions CreateSPOptions()
        {
#if NET45
            var org = new Organization();

            org.Names.Add(new LocalizedName("Kentor.AuthServices", CultureInfo.InvariantCulture));
            org.DisplayNames.Add(new LocalizedName("Kentor AuthServices", CultureInfo.InvariantCulture));
            org.Urls.Add(new LocalizedUri(
                new Uri("http://github.com/KentorIT/authservices"),
                CultureInfo.InvariantCulture));
#endif
            var options = new SPOptions
            {
                EntityId = new Saml2NameIdentifier("https://github.com/KentorIT/authservices"),
#if NET45
                MetadataCacheDuration = new TimeSpan(0, 0, 42),
                MetadataValidDuration = TimeSpan.FromDays(24),
                WantAssertionsSigned = true,
                Organization = org,
#endif
                DiscoveryServiceUrl = new Uri("https://ds.example.com"),
                ReturnUrl = new Uri("https://localhost/returnUrl")
            };

#if NET45
            AddContacts(options);
            AddAttributeConsumingServices(options);
#endif
            return options;
        }

        public static SPOptions CreateSPOptions(Uri publicOrigin)
        {
            var options = CreateSPOptions();
            options.PublicOrigin = publicOrigin;

            return options;
        }

#if NET45
        private static void AddAttributeConsumingServices(SPOptions options)
        {
            var a1 = new RequestedAttribute("urn:attributeName")
            {
                FriendlyName = "friendlyName",
                NameFormat = RequestedAttribute.AttributeNameFormatUri,
                AttributeValueXsiType = ClaimValueTypes.String,
                IsRequired = true
            };
            a1.Values.Add("value1");
            a1.Values.Add("value2");

            var a2 = new RequestedAttribute("someName");

            var acs = new AttributeConsumingService("attributeServiceName")
            {
                IsDefault = true
            };
            acs.RequestedAttributes.Add(a1);
            acs.RequestedAttributes.Add(a2);

            options.AttributeConsumingServices.Add(acs);
        }

        private static void AddContacts(SPOptions options)
        {
            var supportContact = new ContactPerson(ContactType.Support)
            {
                Company = "Kentor",
                GivenName = "Anders",
                Surname = "Abel",
            };

            supportContact.TelephoneNumbers.Add("+46 8 587 650 00");
            supportContact.TelephoneNumbers.Add("+46 708 96 50 63");
            supportContact.EmailAddresses.Add("info@kentor.se");
            supportContact.EmailAddresses.Add("anders.abel@kentor.se");

            options.Contacts.Add(supportContact);
            options.Contacts.Add(new ContactPerson(ContactType.Technical)); // Deliberately void of info.
        }
#endif

        public static IOptions CreateOptions(Func<SPOptions, IOptions> factory)
        {
            var options = factory(CreateSPOptions());

            var idp = new IdentityProvider(new EntityId("https://idp.example.com"), options.SPOptions)
            {
                SingleSignOnServiceUrl = new Uri("https://idp.example.com/idp"),
                SingleLogoutServiceUrl = new Uri("https://idp.example.com/logout"),
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
            };
            idp.SigningKeys.AddConfiguredKey(SignedXmlHelper.TestCert);
            idp.ArtifactResolutionServiceUrls.Add(4660, new )




            KentorAuthServicesSection.Current.IdentityProviders.RegisterIdentityProviders(options);

            return options;
        }

        public static Options CreateOptions()
        {
            return (Options)CreateOptions(sp => new Options(sp));
        }

        private static IOptions CreateOptionsPublicOrigin(Func<SPOptions, IOptions> factory, Uri publicOrigin)
        {
            var options = factory(CreateSPOptions(publicOrigin));

            KentorAuthServicesSection.Current.IdentityProviders.RegisterIdentityProviders(options);
            KentorAuthServicesSection.Current.Federations.RegisterFederations(options);

            return options;
        }
        public static Options CreateOptionsPublicOrigin(Uri publicOrigin)
        {
            return (Options)CreateOptionsPublicOrigin(sp => new Options(sp), publicOrigin);
        }
    }
}
