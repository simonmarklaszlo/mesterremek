using System;

namespace SzivarClubManager.SourceGeneration;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class MockFactoryOfAttribute : FactoryOfAttribute
{
    public MockFactoryOfAttribute(Type modelType) : base(modelType, false) { }
}