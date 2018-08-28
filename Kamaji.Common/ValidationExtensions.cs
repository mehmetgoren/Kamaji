namespace Kamaji.Common
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    //ionix.Data eklenirse bunu çıkar.
    public static class ValidationExtensions
    {
        public static bool IsModelValid<TEntity>(this TEntity entity)
        {
            bool ret = null != entity;
            if (ret)
            {
                foreach (PropertyInfo pi in typeof(TEntity).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    object value = pi.GetValue(entity);

                    foreach (var item in pi.GetCustomAttributes())
                    {
                        ValidationAttribute validation = item as ValidationAttribute;
                        if (null != validation)
                        {
                            if (!validation.IsValid(value))
                            {
                                ret = false;
                                goto Endfunc;
                            }
                        }
                    }
                }
            }

            Endfunc:
            return ret;
        }


        public static bool IsModelListValid<TEntity>(this IEnumerable<TEntity> entityList)
        {
            bool ret = null != entityList && entityList.Any(); 
            if (ret)
            {
                foreach (var entity in entityList)
                {
                    ret = IsModelValid(entity);
                    if (!ret) return false;
                }
            }
            return ret;
        }
    }
}
