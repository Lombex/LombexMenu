using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib.XrefScans;

namespace Utils
{
    public static class XRefScanners
    {
        public static bool XRefScanForMethod(this MethodBase methodBase, string methodName = null, string parentType = null, bool ignoreCase = true)
        {
            if (!string.IsNullOrEmpty(methodName) || !string.IsNullOrEmpty(parentType)) return XrefScanner.XrefScan(methodBase).Any(xref =>
            {
                if (xref.Type != XrefType.Method) return false;
                var found = false;
                MethodBase resolved = xref.TryResolve();
                if (resolved == null) return false;
                if (!string.IsNullOrEmpty(methodName)) found = !string.IsNullOrEmpty(resolved.Name) && resolved.Name.IndexOf(methodName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
                if (!string.IsNullOrEmpty(parentType)) found = !string.IsNullOrEmpty(resolved.ReflectedType?.Name) && resolved.ReflectedType.Name.IndexOf(parentType, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
                return found;
            });
            SetConsoleColor.WriteEmbeddedColorLine($"XRefScanForMethod \"{methodBase}\" has all null/empty parameters. Returning false", SetConsoleColor.ConsoleLogType.Error);
            return false;
        }
        public static bool XRefScanForGlobal(this MethodBase methodBase, string searchTerm, bool ignoreCase = true)
        {
            if (!string.IsNullOrEmpty(searchTerm)) return XrefScanner.XrefScan(methodBase).Any(xref => xref.Type == XrefType.Global && xref.ReadAsObject()?.ToString().IndexOf(searchTerm, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0);
            SetConsoleColor.WriteEmbeddedColorLine($"XRefScanForGlobal \"{methodBase}\" has an empty searchTerm. Returning false", SetConsoleColor.ConsoleLogType.Error);
            return false;
        }
        public static bool CheckMethod(MethodInfo method, string match)
        {
            try
            {
                foreach (var instance in XrefScanner.XrefScan(method)) if (instance.Type == XrefType.Global && instance.ReadAsObject().ToString().Contains(match)) return true;
                return false;
            }
            catch {}
            return false;
        }
        public static bool CheckUsing(MethodInfo method, string methodName, Type type = null)
        {
            foreach (var instance in XrefScanner.XrefScan(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName)) return true;
                    }
                    catch { }
                }
            }
            return false;
        }
        public static bool CheckUsedBy(MethodInfo method, string methodName, Type type = null)
        {
            foreach (var instance in XrefScanner.UsedBy(method))
            {
                if (instance.Type == XrefType.Method)
                {
                    try
                    {
                        if ((type == null || instance.TryResolve().DeclaringType == type) && instance.TryResolve().Name.Contains(methodName)) return true;
                    }
                    catch { }
                }
            }
            return false;
        }
    }
}
