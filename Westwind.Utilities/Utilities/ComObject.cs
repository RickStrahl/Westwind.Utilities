using System;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Westwind.Utilities
{

    /// <summary>
    /// Wrapper around a COM object that allows 'dynamic' like behavior to
    /// work in .NET Core where dynamic with COM objects is not working. This
    ///
    /// Credit to: https://github.com/bubibubi/EntityFrameworkCore.Jet/blob/3.1-preview/src/System.Data.Jet/ComObject.cs
    /// </summary>
    public class ComObject : DynamicObject, IDisposable
    {
        internal object _instance;

#if DEBUG
        private readonly Guid Id = Guid.NewGuid();
#endif

        /// <summary>
        /// Create a new instance based on ProgId
        /// </summary>
        /// <param name="progid"></param>
        /// <returns></returns>
        public static ComObject CreateFromProgId(string progid)
        {
            var type = Type.GetTypeFromProgID(progid, false);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    return new ComObject(instance);
                }
            }

            throw new TypeLoadException("Couldn't create COM Wrapper for: " + progid);
        }

        /// <summary>
        /// Create a new instance based on ClassId
        /// </summary>
        /// <param name="clsid">Guid class Id</param>
        /// <returns></returns>
        public static ComObject CreateFirstFrom(Guid clsid)
        {
            var type = Type.GetTypeFromCLSID(clsid, false);
            if (type != null)
            {
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    return new ComObject(instance);
                }
            }

            throw new TypeLoadException("Couldn't create COM Wrapper for: " + clsid);
        }

        public ComObject(object instance)
        {
            if (instance is ComObject)
                _instance = ((ComObject)instance)._instance;

            _instance = instance;
        }

        /// <summary>
        /// Creates a new instance based on a ProgId
        /// </summary>
        /// <param name="progid"></param>
        public ComObject(string progid)
            : this(Activator.CreateInstance(Type.GetTypeFromProgID(progid, true)))
        {
        }

        /// <summary>
        /// Creates an instance based on a class Id
        /// </summary>
        /// <param name="clsid"></param>
        public ComObject(Guid clsid)
            : this(Activator.CreateInstance(Type.GetTypeFromCLSID(clsid, true)))
        {
        }

        /// <summary>
        /// Removes the COM reference linkage from this object
        /// </summary>
        /// <returns></returns>
        public object Detach()
        {
            var instance = _instance;
            _instance = null;
            return instance;
        }


        #region DynamicObject implementation

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = WrapIfRequired(
                _instance.GetType()
                    .InvokeMember(
                        binder.Name,
                        BindingFlags.GetProperty,
                        Type.DefaultBinder,
                        _instance,
                        new object[0]
                    ));
            return true;
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _instance.GetType()
                .InvokeMember(
                    binder.Name,
                    BindingFlags.SetProperty,
                    Type.DefaultBinder,
                    _instance,
                    new[]
                    {
                            value is ComObject comObject
                                ? comObject._instance
                                : value
                    }
                );
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = WrapIfRequired(
                _instance.GetType()
                    .InvokeMember(
                        binder.Name,
                        BindingFlags.InvokeMethod,
                        Type.DefaultBinder,
                        _instance,
                        args
                    ));
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            // This should work for all specific interfaces derived from `_Collection` (like `_Tables`) in ADOX.
            result = WrapIfRequired(
                _instance.GetType()
                    .InvokeMember(
                        "Item",
                        BindingFlags.GetProperty,
                        Type.DefaultBinder,
                        _instance,
                        indexes
                    ));
            return true;
        }

        #endregion



        // See https://github.com/dotnet/runtime/issues/12587#issuecomment-578431424
        
        /// <summary>
        /// Wrap any embedded raw COM objects in a new ComObject wrapper as well
        /// when returned from members or method results.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static object WrapIfRequired(object obj)
            => obj != null && obj.GetType().IsCOMObject ? new ComObject(obj) : obj;

        public void Dispose()
        {
            // The RCW is a .NET object and cannot be released from the finalizer,
            // because it might not exist anymore.
            if (_instance != null)
            {
                Marshal.ReleaseComObject(_instance);
                _instance = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}