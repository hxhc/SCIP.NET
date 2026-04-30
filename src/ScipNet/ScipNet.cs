// SCIP.NET - A modern C# wrapper for the SCIP optimization solver
// Copyright (c) 2024 SCIP.NET Contributors
// Licensed under the Apache License, Version 2.0

global using ScipNet.Core;
global using ScipNet.Native;

namespace ScipNet;

/// <summary>
/// SCIP.NET - A modern C# wrapper for the SCIP optimization solver
/// </summary>
// SCIP.NET - SCIP优化求解器的现代C#封装库
public static class ScipNet
{
    /// <summary>
    /// Get SCIP.NET version
    /// </summary>
    // 获取SCIP.NET版本号
    public static string Version => "0.1.0";

    /// <summary>
    /// Get SCIP version
    /// </summary>
    // 获取SCIP版本号
    public static string ScipVersion => "9.0.0";
}
