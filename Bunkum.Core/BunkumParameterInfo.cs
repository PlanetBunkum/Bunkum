using System.Reflection;

namespace Bunkum.Core;

/// <summary>
/// A parameter used for injection information.
/// </summary>
/// <seealso cref="ParameterInfo"/>
public class BunkumParameterInfo
{
    /// <summary>
    /// Instantiates a set of parameter info.
    /// </summary>
    /// <param name="parameterType">The parameter's type.</param>
    /// <param name="name">The parameter's name.</param>
    public BunkumParameterInfo(Type parameterType, string name)
    {
        this.ParameterType = parameterType;
        this.Name = name;
    }
    
    /// <summary>
    /// Instantiates a set of parameter info from existing reflection data.
    /// </summary>
    /// <param name="parameter">The reflection data to use.</param>
    public BunkumParameterInfo(ParameterInfo parameter)
    {
        this.ParameterType = parameter.ParameterType;
        this.Name = parameter.Name;
    }
    
    /// <summary>
    /// The type of the parameter.
    /// </summary>
    public Type ParameterType { get; }
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string? Name { get; }
}