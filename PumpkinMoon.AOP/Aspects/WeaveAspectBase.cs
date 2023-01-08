using System;

namespace PumpkinMoon.AOP.Aspects
{
    public abstract class WeaveAspectBase : Attribute
    {
        internal abstract void WeaveAspect(WeaveArgs args);
    }
}