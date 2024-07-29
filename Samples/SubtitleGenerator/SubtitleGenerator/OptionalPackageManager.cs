using System;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.Storage.Packaging.Appx;
using Windows.Win32.Security;
using Microsoft.Win32.SafeHandles;
using Microsoft.Windows.ApplicationModel.DynamicDependency;


// https://github.com/microsoft/WindowsAppSDK/blob/main/dev/DynamicDependency/Powershell/MsixDynamicDependency.psm1

namespace SubtitleGenerator
{

    class HeapFreeSafeHandle : SafeHandle
    {
        public unsafe HeapFreeSafeHandle(void* value) : base(new IntPtr(value), true)
        {
        }

        public override bool IsInvalid => this.handle.ToInt64() == 0;

        protected override bool ReleaseHandle()
        {
            unsafe
            {
                return PInvoke.HeapFree(PInvoke.GetProcessHeap(), Windows.Win32.System.Memory.HEAP_FLAGS.HEAP_NO_SERIALIZE, this.handle.ToPointer());
            }
        }
    }
    public enum CreatePackageDependencyOptions
    {
        None = 0,

        // Disable dependency resolution when pinning a package dependency.
        DoNotVerifyDependencyResolution = 0x00000001,

        // Define the package dependency for the system, accessible to all users
        // (default is the package dependency is defined for a specific user).
        // This option requires the caller has adminitrative privileges.
        ScopeIsSystem = 0x00000002,
    }

    public enum PackageDependencyLifetimeKind
    {
        // The current process is the lifetime artifact. The package dependency
        // is implicitly deleted when the process terminates.
        Process = 0,

        // The lifetime artifact is an absolute filename or path.
        // The package dependency is implicitly deleted when this is deleted.
        FilePath = 1,

        // The lifetime artifact is a registry key in the format
        // 'root\\subkey' where root is one of the following: HKLM, HKCU, HKCR, HKU.
        // The package dependency is implicitly deleted when this is deleted.
        RegistryKey = 2,
    }

    public enum AddPackageDependencyOptions
    {
        None = 0,
        PrependIfRankCollision = 0x00000001,
    };

    public class Rank
    {
        public const int Default = 0;
    }

    public enum PackageDependencyProcessorArchitectures
    {
        None = 0,
        Neutral = 0x00000001,
        X86 = 0x00000002,
        X64 = 0x00000004,
        Arm = 0x00000008,
        Arm64 = 0x00000010,
        X86A64 = 0x00000020,
    };

    public class DynDep
    {
        [DllImport("kernelbase.dll", EntryPoint = "GetProcessHeap", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr kernel32_GetProcessHeap();

        [DllImport("kernelbase.dll", EntryPoint = "HeapFree", ExactSpelling = true, SetLastError = true)]
        private static extern bool kernel32_HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        private static bool HeapFree(IntPtr p)
        {
            return kernel32_HeapFree(kernel32_GetProcessHeap(), 0, p);
        }

        //private static unsafe int TryCreatePackageDependency(
        //    string packageFamilyName,
        //    PackageVersion minVersion,
        //    PackageDependencyProcessorArchitectures packageDependencyProcessorArchitectures,
        //    PackageDependencyLifetimeKind lifetimeKind,
        //    string lifetimeArtifact,
        //    int options,
        //    out PWSTR packageDependencyId)
        //{
        //    PACKAGE_VERSION version = new PACKAGE_VERSION();
        //    version.Anonymous.Anonymous.Major = minVersion.Major;
        //    version.Anonymous.Anonymous.Minor = minVersion.Minor;
        //    version.Anonymous.Anonymous.Revision = minVersion.Revision;
        //    version.Anonymous.Anonymous.Build = minVersion.Build;

        //    return PInvoke.TryCreatePackageDependency(
        //        new SafeFileHandle(IntPtr.Zero, true),
        //        packageFamilyName,
        //        version,
        //        (Windows.Win32.Storage.Packaging.Appx.PackageDependencyProcessorArchitectures)packageDependencyProcessorArchitectures,
        //        (Windows.Win32.Storage.Packaging.Appx.PackageDependencyLifetimeKind)lifetimeKind,
        //        lifetimeArtifact,
        //        (Windows.Win32.Storage.Packaging.Appx.CreatePackageDependencyOptions)options,
        //        out packageDependencyId);

        //}

        //public static int TryCreate(
        //    string packageFamilyName,
        //    PackageVersion minVersion,
        //    PackageDependencyProcessorArchitectures packageDependencyProcessorArchitectures,
        //    PackageDependencyLifetimeKind lifetimeKind,
        //    string lifetimeArtifact,
        //    int options,
        //    out string packageDependencyId)
        //{
        //    packageDependencyId = null;

        //    unsafe
        //    {
        //        int hr = TryCreatePackageDependency(
        //            packageFamilyName,
        //            minVersion,
        //            packageDependencyProcessorArchitectures,
        //            lifetimeKind,
        //            lifetimeArtifact,
        //            options,
        //            out var pdi);

        //        if (hr >= 0)
        //        {
        //            packageDependencyId = Marshal.PtrToStringUni(new IntPtr(pdi));
        //        }
        //        if (new IntPtr(pdi) != IntPtr.Zero)
        //        {
        //            HeapFree(new IntPtr(pdi));
        //        }
        //        return hr;
        //    }
        //}

        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int TryCreatePackageDependency(
           /*PSID*/ IntPtr user,
           string packageFamilyName,
           /*PACKAGE_VERSION*/ long minVersion,
           /*PackageDependencyProcessorArchitectures*/ int packageDependencyProcessorArchitectures,
           /*PackageDependencyLifetimeKind*/ int lifetimeKind,
           string lifetimeArtifact,
           /*CreatePackageDependencyOptions*/ int options,
           /*_Outptr_result_maybenull_ PWSTR* */ out IntPtr packageDependencyId);

        public static int TryCreate(
            string packageFamilyName,
            long minVersion,
            /*PackageDependencyProcessorArchitectures*/ int packageDependencyProcessorArchitectures,
            /*PackageDependencyLifetimeKind*/ int lifetimeKind,
            string lifetimeArtifact,
            /*CreatePackageDependencyOptions*/ int options,
            /*_Outptr_result_maybenull_ PWSTR* */ out string packageDependencyId)
        {
            packageDependencyId = null;

            IntPtr pdi = IntPtr.Zero;
            int hr = TryCreatePackageDependency(IntPtr.Zero, packageFamilyName, minVersion, packageDependencyProcessorArchitectures,
                                                lifetimeKind, lifetimeArtifact, options, out pdi);
            if (hr >= 0)
            {
                packageDependencyId = Marshal.PtrToStringUni(pdi);
            }
            if (pdi != IntPtr.Zero)
            {
                HeapFree(pdi);
            }
            return hr;
        }

        // Undefine a package dependency. Removing a pin on a PackageDependency is typically done at uninstall-time.
        // This implicitly occurs if the package dependency's 'lifetime artifact' (specified via TryCreatePackageDependency)
        // is deleted. Packages that are not referenced by other packages and have no pins are elegible to be removed.
        //
        // @warn DeletePackageDependency() requires the caller have administrative privileges
        //       if the package dependency was pinned with CreatePackageDependencyOptions_ScopeIsSystem.
        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int DeletePackageDependency(
            string packageDependencyId);

        public static int Delete(
            string packageDependencyId)
        {
            return DeletePackageDependency(packageDependencyId);
        }

        // Resolve a previously-pinned PackageDependency to a specific package and
        // add it to the invoking process' package graph. Once the dependency has
        // been added other code-loading methods (LoadLibrary, CoCreateInstance, etc)
        // can find the binaries in the resolved package.
        //
        // Package resolution is specific to a user and can return different values
        // for different users on a system.
        //
        // Each successful AddPackageDependency() adds the resolve packaged to the
        // calling process' package graph, even if already present. There is no
        // duplicate 'detection' or 'filtering' applied by the API (multiple
        // references from a package is not harmful). Once resolution is complete
        // the package dependency stays resolved for that user until the last reference across
        // all processes for that user is removed via RemovePackageDependency (or
        // process termination).
        //
        // AddPackageDependency() adds the resolved package to the caller's package graph,
        // per the rank specified. A process' package graph is a list of packages sorted by
        // rank in ascending order (-infinity...0...+infinity). If package(s) are present in the
        // package graph with the same rank as the call to AddPackageDependency the resolved
        // package is (by default) added after others of the same rank. To add a package
        // before others of the same rank, specify AddPackageDependencyOptions_PrependIfRankCollision.
        //
        // Every AddPackageDependency can be balanced by a RemovePackageDependency
        // to remove the entry from the package graph. If the process terminates all package
        // references are removed, but any pins stay behind.
        //
        // AddPackageDependency adds the resolved package to the process' package
        // graph, per the rank and options parameters. The process' package
        // graph is used to search for DLLs (per Dynamic-Link Library Search Order),
        // WinRT objects and other resources; the caller can now load DLLs, activate
        // WinRT objects and use other resources from the framework package until
        // RemovePackageDependency is called. The packageDependencyId parameter
        // must match a package dependency defined for the calling user or the
        // system (i.e. pinned with CreatePackageDependencyOptions_ScopeIsSystem) else
        // an error is returned.
        //
        // @param packageDependencyContext valid until passed to RemovePackageDependency()
        // @param packageFullName allocated via HeapAlloc; use HeapFree to deallocate
        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int AddPackageDependency(
            string packageDependencyId,
            int rank,
            /*AddPackageDependencyOptions*/ int options,
            /*_Out_ PACKAGEDEPENDENCY_CONTEXT* */ out IntPtr packageDependencyContext,
            /*_Outptr_opt_result_maybenull_ PWSTR* */ out IntPtr packageFullName);

        public static int Add(
            string packageDependencyId,
            int rank,
            /*AddPackageDependencyOptions*/ int options,
            /*_Out_ PACKAGEDEPENDENCY_CONTEXT* */ out IntPtr packageDependencyContext,
            out string packageFullName)
        {
            packageDependencyContext = IntPtr.Zero;
            packageFullName = null;

            IntPtr pdc = IntPtr.Zero;
            IntPtr pfn = IntPtr.Zero;
            int hr = AddPackageDependency(packageDependencyId, rank, options, out pdc, out pfn);
            if (hr >= 0)
            {
                packageDependencyContext = pdc;
                packageFullName = Marshal.PtrToStringUni(pfn);
            }
            if (pfn != IntPtr.Zero)
            {
                HeapFree(pfn);
            }
            return hr;
        }

        // Remove a resolved PackageDependency from the current process' package graph
        // (i.e. undo AddPackageDependency). Used at runtime (i.e. the moral equivalent
        // of Windows' RemoveDllDirectory()).
        //
        // @note This does not unload loaded resources (DLLs etc). After removing
        //        a package dependency any files loaded from the package can continue
        //        to be used; future file resolution will fail to see the removed
        //        package dependency.
        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int RemovePackageDependency(
            /*PACKAGEDEPENDENCY_CONTEXT*/ IntPtr packageDependencyContext);

        public static int Remove(
            IntPtr packageDependencyContext)
        {
            return RemovePackageDependency(packageDependencyContext);
        }

        // Return the package full name that would be used if the
        // PackageDependency were to be resolved. Does not add the
        // package to the process graph.
        //
        // @param packageFullName allocated via HeapAlloc; use HeapFree to deallocate.
        //                        If the package dependency cannot be resolved the function
        //                        succeeds but packageFullName is nullptr.
        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GetResolvedPackageFullNameForPackageDependency(
            string packageDependencyId,
            /*_Outptr_result_maybenull_ PWSTR* */ out IntPtr packageFullName);

        public static int GetResolvedPackageFullName(
            string packageDependencyId,
            out string packageFullName)
        {
            packageFullName = null;

            IntPtr pfn = IntPtr.Zero;
            int hr = GetResolvedPackageFullNameForPackageDependency(packageDependencyId, out pfn);
            if (hr >= 0)
            {
                packageFullName = Marshal.PtrToStringUni(pfn);
            }
            if (pfn != IntPtr.Zero)
            {
                HeapFree(pfn);
            }
            return hr;
        }

        // Return the package dependency for the context.
        //
        // @param packageDependencyId allocated via HeapAlloc; use HeapFree to deallocate.
        //                            If the package dependency context cannot be resolved
        //                            the function succeeds but packageDependencyId is nullptr.
        [DllImport("kernelbase.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        private static extern int GetIdForPackageDependencyContext(
            /*PACKAGEDEPENDENCY_CONTEXT*/ IntPtr packageDependencyContext,
            /*_Outptr_result_maybenull_ PWSTR* */ out IntPtr packageDependencyId);

        public static int GetIdForContext(
            /*PACKAGEDEPENDENCY_CONTEXT*/ IntPtr packageDependencyContext,
            /*_Outptr_result_maybenull_ PWSTR* */ out string packageDependencyId)
        {
            packageDependencyId = null;

            IntPtr pdi = IntPtr.Zero;
            int hr = GetIdForPackageDependencyContext(packageDependencyContext, out pdi);
            if (hr >= 0)
            {
                packageDependencyId = Marshal.PtrToStringUni(pdi);
            }
            if (pdi != IntPtr.Zero)
            {
                HeapFree(pdi);
            }
            return hr;
        }
    }

    public class PackageGraph
    {
        // Returns the package graph's current revision ID.
        [DllImport("kernelbase.dll")]
        private static extern uint GetPackageGraphRevisionId();

        public static uint RevisionId
        {
            get
            {
                return GetPackageGraphRevisionId();
            }
        }
    }
    //public enum CreatePackageDependencyOptions
    //{
    //    None = 0,
    //    DoNotVerifyDependencyResolution = 0x00000001,
    //    ScopeIsSystem = 0x00000002,
    //}

    //public enum PackageDependencyLifetimeKind
    //{
    //    Process = 0,
    //    FilePath = 1,
    //    RegistryKey = 2,
    //}

    //public enum AddPackageDependencyOptions
    //{
    //    None = 0,
    //    PrependIfRankCollision = 0x00000001,
    //};

    //public class Rank
    //{
    //    public const int Default = 0;
    //}

    //public enum PackageDependencyProcessorArchitectures
    //{
    //    None = 0,
    //    Neutral = 0x00000001,
    //    X86 = 0x00000002,
    //    X64 = 0x00000004,
    //    Arm = 0x00000008,
    //    Arm64 = 0x00000010,
    //    X86A64 = 0x00000020,
    //};

    //public class PackageDependency
    //{
    //    private static bool HeapFree(IntPtr p)
    //    {
    //        unsafe{
    //            return PInvoke.HeapFree(PInvoke.GetProcessHeap(), HEAP_FLAGS.HEAP_NO_SERIALIZE, p.ToPointer());
    //        }
    //    }

    //    private static unsafe int TryCreatePackageDependency(
    //        string packageFamilyName,
    //        Microsoft.Windows.ApplicationModel.DynamicDependency.PackageVersion minVersion,
    //        PackageDependencyProcessorArchitectures packageDependencyProcessorArchitectures,
    //        PackageDependencyLifetimeKind lifetimeKind,
    //        string lifetimeArtifact,
    //        int options,
    //        out PWSTR packageDependencyId)
    //    {
    //        PACKAGE_VERSION version = new PACKAGE_VERSION();
    //        version.Anonymous.Anonymous.Major = minVersion.Major;
    //        version.Anonymous.Anonymous.Minor = minVersion.Minor;
    //        version.Anonymous.Anonymous.Revision = minVersion.Revision;
    //        version.Anonymous.Anonymous.Build = minVersion.Build;

    //        return PInvoke.TryCreatePackageDependency(
    //            new SafeFileHandle(IntPtr.Zero, true),
    //            packageFamilyName,
    //            version,
    //            (Windows.Win32.Storage.Packaging.Appx.PackageDependencyProcessorArchitectures)packageDependencyProcessorArchitectures,
    //            (Windows.Win32.Storage.Packaging.Appx.PackageDependencyLifetimeKind)lifetimeKind,
    //            lifetimeArtifact,
    //            (Windows.Win32.Storage.Packaging.Appx.CreatePackageDependencyOptions)options,
    //            out packageDependencyId);

    //    }

    //    public static int TryCreate(
    //        string packageFamilyName,
    //        Microsoft.Windows.ApplicationModel.DynamicDependency.PackageVersion minVersion,
    //        PackageDependencyProcessorArchitectures packageDependencyProcessorArchitectures,
    //        PackageDependencyLifetimeKind lifetimeKind,
    //        string lifetimeArtifact,
    //        int options,
    //        out string packageDependencyId)
    //    {
    //        packageDependencyId = null;

    //        unsafe
    //        {
    //            int hr = TryCreatePackageDependency(
    //                packageFamilyName,
    //                minVersion,
    //                packageDependencyProcessorArchitectures,
    //                lifetimeKind,
    //                lifetimeArtifact,
    //                options,
    //                out var pdi);

    //            if (hr >= 0)
    //            {
    //                packageDependencyId = Marshal.PtrToStringUni(new IntPtr(pdi));
    //            }
    //            if (new IntPtr(pdi) != IntPtr.Zero)
    //            {
    //                HeapFree(new IntPtr(pdi));
    //            }
    //            return hr;
    //        }
    //    }

    //    private static unsafe int DeletePackageDependency(PWSTR packageDependencyId)
    //    {
    //        return PInvoke.DeletePackageDependency((char*)packageDependencyId);
    //    }

    //    public static int Delete(string packageDependencyId)
    //    {
    //        unsafe
    //        {
    //            fixed (char* packageDependencyIdPtr = packageDependencyId)
    //            {
    //                return DeletePackageDependency((PWSTR)packageDependencyIdPtr);
    //            }
    //        }
    //    }

    //    private static unsafe int AddPackageDependency(
    //        string packageDependencyId,
    //        int rank,
    //        int options,
    //        out PackageDependencyContext packageDependencyContext,
    //        PWSTR packageFullName)
    //    {
    //        return PInvoke.AddPackageDependency(
    //            packageDependencyId,
    //            rank,
    //            (Windows.Win32.Storage.Packaging.Appx.AddPackageDependencyOptions)options,
    //            out packageDependencyContext,
    //            &packageFullName);
    //    }

    //    public static int Add(
    //        string packageDependencyId,
    //        int rank,
    //        int options,
    //        out PackageDependencyContext packageDependencyContext,
    //        out string packageFullName)
    //    {
    //        packageDependencyContext = new();


    //        unsafe
    //        {

    //            fixed (PWSTR* packageFullNamePtr = (PWSTR)packageFullName)
    //            {

    //                int hr = AddPackageDependency(
    //                    packageDependencyId,
    //                    rank,
    //                    options,
    //                    out packageDependencyContext,
    //                    packageFullNamePtr);

    //                if (hr >= 0)
    //                {
    //                    packageFullName = Marshal.PtrToStringUni((IntPtr)packageFullNamePtr);
    //                }
    //                if (packageFullNamePtr != IntPtr.Zero)
    //                {
    //                    HeapFree((IntPtr)packageFullNamePtr);
    //                }
    //                return hr;
    //            }
    //        }
    //    }

    //    private static unsafe int RemovePackageDependency(PACKAGEDEPENDENCY_CONTEXT packageDependencyContext)
    //    {
    //        return PInvoke.RemovePackageDependency(packageDependencyContext);
    //    }

    //    public static int Remove(PackageDependencyContext packageDependencyContext)
    //    {
    //        unsafe
    //        {
    //            return RemovePackageDependency((PACKAGEDEPENDENCY_CONTEXT)packageDependencyContext);
    //        }
    //    }

    //    private static unsafe int GetResolvedPackageFullNameForPackageDependency(
    //        PWSTR packageDependencyId,
    //        out PWSTR packageFullName)
    //    {
    //        return PInvoke.GetResolvedPackageFullNameForPackageDependency(packageDependencyId, out packageFullName);
    //    }

    //    public static int GetResolvedPackageFullName(string packageDependencyId, out string packageFullName)
    //    {
    //        packageFullName = null;

    //        unsafe
    //        {
    //            fixed (char* packageDependencyIdPtr = packageDependencyId)
    //            {
    //                int hr = GetResolvedPackageFullNameForPackageDependency((PWSTR)packageDependencyIdPtr, out var packageFullNamePtr);

    //                if (hr >= 0)
    //                {
    //                    packageFullName = Marshal.PtrToStringUni((IntPtr)packageFullNamePtr);
    //                }
    //                if (packageFullNamePtr != IntPtr.Zero)
    //                {
    //                    HeapFree((IntPtr)packageFullNamePtr);
    //                }
    //                return hr;
    //            }
    //        }
    //    }

    //    private static unsafe int GetIdForPackageDependencyContext(
    //        PackageDependencyContext packageDependencyContext,
    //        out PWSTR packageDependencyId)
    //    {
    //        return PInvoke.GetIdForPackageDependencyContext(packageDependencyContext, out packageDependencyId);
    //    }

    //    public static int GetIdForContext(
    //        PackageDependencyContext packageDependencyContext,

    //        out string packageDependencyId)
    //    {
    //        packageDependencyId = null;

    //        unsafe
    //        {
    //            int hr = GetIdForPackageDependencyContext(packageDependencyContext, out var packageDependencyIdPtr);

    //            if (hr >= 0)
    //            {
    //                packageDependencyId = Marshal.PtrToStringUni((IntPtr)packageDependencyIdPtr);
    //            }
    //            if (packageDependencyIdPtr != IntPtr.Zero)
    //            {
    //                HeapFree((IntPtr)packageDependencyIdPtr);
    //            }
    //            return hr;
    //        }
    //    }
    //}

    //public class PackageGraph
    //{
    //    public static uint RevisionId
    //    {
    //        get
    //        {
    //            return PInvoke.GetPackageGraphRevisionId();
    //        }
    //    }
    //}
}
