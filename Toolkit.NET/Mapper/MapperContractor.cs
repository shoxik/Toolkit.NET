﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolkit.NET.Extensions.Type;

namespace Toolkit.NET.Mapper
{
    /// <summary>
    /// This class contains rules for mapping values from TMapFrom instance to TMapTo instance
    /// </summary>
    /// <typeparam name="TMapFrom"></typeparam>
    /// <typeparam name="TMapTo"></typeparam>
    public class MapperContractor<TMapFrom, TMapTo>
    {
        public List<MapperSpectialContract<TMapFrom, TMapTo>> Contracts { get; private set; }

        /// <summary>
        /// Constructor for MapperContractor
        /// Exception: If one of the parameters is not correct
        /// </summary>
        /// <param name="paContracts"></param>
        public MapperContractor(params MapperSpectialContract<TMapFrom, TMapTo>[] paContracts)
        {
            Contracts = new List<MapperSpectialContract<TMapFrom, TMapTo>>();
            var fromProperties = typeof(TMapFrom).GetProperties();
            var toProperties = typeof(TMapTo).GetProperties();

            foreach (var fromProperty in fromProperties)
            {
                var contracts = paContracts.Where(c => c.MapFromProperty == fromProperty.Name).ToList();
                if (!contracts.Any())
                {
                    var toProperty = toProperties.FirstOrDefault(n => String.Equals(n.Name, fromProperty.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(toProperty?.Name))
                        Contracts.Add(new MapperSpectialContract<TMapFrom, TMapTo>(fromProperty.Name, toProperty.Name,
                            (fromObject, toObject) =>
                            {
                                object valueFrom = this.ConvertType(fromProperty.GetValue(fromObject),
                                    toProperty.PropertyType);
                                toProperty.SetValue(toObject, valueFrom);
                            }));
                }
                else
                    Contracts.AddRange(contracts);
            }
        }

        /// <summary>
        /// Method automatically converts standart types
        /// </summary>
        /// <param name="paFromValue"></param>
        /// <param name="paToValueType"></param>
        /// <returns></returns>
        public object ConvertType(object paFromValue, Type paToValueType)
        {
            if (paFromValue == null)
                return paToValueType.GetDefaultValue();
            return paFromValue.GetType() == paToValueType ? paFromValue : System.Convert.ChangeType(paFromValue, paToValueType);
        }

        /// <summary>
        /// Method which maps properties from paMapFrom instance to paMapTo instance
        /// If is one of the instances null then method will do nothing
        /// </summary>
        /// <param name="paMapFrom"></param>
        /// <param name="paMapTo"></param>
        public void MapTypes(ref TMapFrom paMapFrom, ref TMapTo paMapTo)
        {
            if (paMapFrom == null || paMapTo == null)
                return;
            MapProperties(ref paMapFrom, ref paMapTo);
        }

        private void MapProperties(ref TMapFrom paMapFrom, ref TMapTo paMapTo)
        {
            var propertiesFrom = paMapFrom.GetType().GetProperties();

            foreach (var propertyInfo in propertiesFrom)
            {
                var contracts = Contracts.Where(c => c.MapFromProperty == propertyInfo.Name);
                if (!Contracts.Any())
                    contracts = Contracts.Where(c => c.MapFromProperty.ToLower() == propertyInfo.Name.ToLower());
                foreach (var contract in contracts)
                {
                    contract.Contract(paMapFrom, paMapTo);
                }
            }
        }
    }
}
