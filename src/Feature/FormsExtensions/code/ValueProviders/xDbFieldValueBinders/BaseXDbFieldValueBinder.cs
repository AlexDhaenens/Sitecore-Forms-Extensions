﻿using System;
using Feature.FormsExtensions.ValueProviderConditions;
using Feature.FormsExtensions.XDb;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.DependencyInjection;
using Sitecore.ExperienceForms.ValueProviders;
using Sitecore.XConnect;

namespace Feature.FormsExtensions.ValueProviders.xDbFieldValueBinders
{
    public abstract class BaseXDbFieldValueBinder<T> : IFieldValueBinder where T : Facet
    {
        protected abstract string GetFacetKey();
        protected abstract IFieldValueBinderResult GetFieldBindingValueFromFacet(T facet);
        protected abstract T CreateFacet();
        private readonly IXDbService xDbService;
        
        protected BaseXDbFieldValueBinder()
        {
           xDbService = ServiceLocator.ServiceProvider.GetService<IXDbService>();
        }

        public IFieldValueBinderResult GetBindingValue()
        {
            if (Tracker.Current?.Contact == null)
                return new NoFieldValueBindingFoundResult();
            var xConnectFacet = Tracker.Current.Contact.GetFacet<IXConnectFacets>("XConnectFacets");
            if (xConnectFacet == null)
            {
                return new NoFieldValueBindingFoundResult();
            }
            if (xConnectFacet.Facets == null)
            {
                return new NoFieldValueBindingFoundResult();
            }
            if (!xConnectFacet.Facets.ContainsKey(GetFacetKey()))
            {
                return new NoFieldValueBindingFoundResult();
            }
            if (!(xConnectFacet.Facets[GetFacetKey()] is T facet))
            {
                return new NoFieldValueBindingFoundResult();
            }
            return GetFieldBindingValueFromFacet(facet);
        }
        
        public abstract void StoreValue(object newValue);

        protected void UpdateFacet(Action<T> updateFacet)
        {
            xDbService.UpdateCurrentContactFacet(GetFacetKey(), updateFacet, CreateFacet);
        }

        public virtual object GetValue(string parameters)
        {
            if (!ValueProviderConditionsContext.ValueProviderConditionsMet)
                return string.Empty;
            var bindingValue = GetBindingValue();
            return bindingValue.HasValue() ? bindingValue.Value : string.Empty;
        }

        public FieldValueProviderContext ValueProviderContext { get; set; }
    }
}