// SCIP.NET - A modern C# wrapper for the SCIP optimization solver
// Copyright (c) 2024 SCIP.NET Contributors
// Licensed under the Apache License, Version 2.0

global using ScipNet.Core;
global using ScipNet.Native;

namespace ScipNet;

/// <summary>
/// SCIP.NET - A modern C# wrapper for the SCIP optimization solver
/// </summary>
public static class ScipNet
{
    /// <summary>
    /// 获取 SCIP.NET 版本
    /// </summary>
    public static string Version => "0.1.0";

    /// <summary>
    /// 获取 SCIP 版本
    /// </summary>
    public static string ScipVersion => "9.0.0";
}
