using UnityEngine;
using System;
using System.Collections;



public static class Easing
{
    #region Public Enum's
    
    public enum Method 
    {
        None = 0,
        
        Linear,
        
        CubicIn,
        CubicOut,
        CubicInOut,
        
        QuadIn,
        QuadOut,
        QuadInOut,
        
        QuartIn,
        QuartOut,
        QuartInOut,
        
        QuintIn,
        QuintOut,
        QuintInOut,
        
        SineIn,
        SineOut,
        SineInOut,
        
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        
        CircIn,
        CircOut,
        CircInOut,
        
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        
        BackIn,
        BackOut,
        BackInOut,
        
        BounceIn,
        BounceOut,
        BounceInOut
    };
    
    #endregion
    
    #region Defines
    
    [System.Serializable]
    public class Helper
    {
        #region Public Variables
        
        public float time = 1.0f;
        public Method method = Method.None;
        public WrapMode wrap = WrapMode.Default;
        
        #endregion
        
        #region Private Variables
        
        private float _timer;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public Helper ()
        {}
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Easing.Helper"/> class.
        /// </summary>
        /// <param name='method'>
        /// Method.
        /// </param>
        /// <param name='time'>
        /// Time.
        /// </param>
        public Helper ( Method method, float time )
        {
            this.time = time;
            this.method = method;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Easing.Helper"/> class.
        /// </summary>
        /// <param name='method'>
        /// Method.
        /// </param>
        /// <param name='time'>
        /// Time.
        /// </param>
        /// <param name='wrap'>
        /// Wrap.
        /// </param>
        public Helper ( Method method, float time, WrapMode wrap )
        {
            this.time = time;
            this.method = method;
            this.wrap = wrap;
        }
        
        #endregion
        
        #region Public Functions 
        
        public void Reset ()
        {
            _timer = 0.0f;
        }
        
        public bool Update ( float deltaTime, float startingValue, float targetValue, out float result )
        {
            if ( _timer > time )
            {
                result = targetValue;
                return false;
            }
            else
            {
                _timer += deltaTime;
                result = Easing.Ease ( WrapTime ( _timer ), startingValue, targetValue, time, method );
                return true;
            }
        }
        
        public bool Update ( float deltaTime, Vector2 startingValue, Vector2 targetValue, out Vector2 result )
        {
            if ( _timer > time )
            {
                result = targetValue;
                return false;
            }
            else
            {
                _timer += deltaTime;
                result = Easing.Ease ( WrapTime ( _timer ), startingValue, targetValue, time, method );
                return true;
            }
        }
        
        public bool Update ( float deltaTime, Vector3 startingValue, Vector3 targetValue, out Vector3 result )
        {
            if ( _timer > time )
            {
                result = targetValue;
                return false;
            }
            else
            {
                _timer += deltaTime;
                result = Easing.Ease ( WrapTime ( _timer ), startingValue, targetValue, time, method );
                return true;
            }
        }
        
        #endregion
        
        #region Private Functions
        
        /// <summary>
        /// Wrap a float value between the min and max
        /// </summary>
        /// <param name="val"> start value </param>
        /// <returns> A Wrapped Value. </returns>
        public float WrapTime ( float val )
        {
            switch( wrap )
            {
                case WrapMode.Default:
                case WrapMode.Clamp:
                case WrapMode.ClampForever:
                {
                    return Mathf.Min ( val, time );
                }
                case WrapMode.Loop:
                {
                    return Mathf.Repeat ( val, time );
                }
                case WrapMode.PingPong:
                {
                    return Mathf.PingPong ( val, time );
                }
                default:
                    return val;
            }
        }
        
        #endregion
        
    }
    
    #endregion
    
    #region Easing Delegate
    
    // ----------------------------------------------------------------------------
    // Easing Deletage
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public delegate float EaseingDelgate ( float t, float b, float c, float d );
    
    #endregion
    
    #region Static Variables
    
    private static EaseingDelgate[] delgates;
    
    #endregion
    
    #region Static Constructor
    
    // ----------------------------------------------------------------------------
    // Static Constructor to create a cache.
    // ----------------------------------------------------------------------------
    static Easing ()
    {
        
        int size = Enum.GetValues ( typeof ( Easing.Method ) ).Length;
        
        delgates = new EaseingDelgate[size];
        
        delgates[(int)Method.None] = new Easing.EaseingDelgate(None);
        
        delgates[(int)Method.Linear] = new Easing.EaseingDelgate(Linear);
        
        delgates[(int)Method.BackIn] = new Easing.EaseingDelgate(Back.EaseIn);
        delgates[(int)Method.BackOut] = new Easing.EaseingDelgate(Back.EaseOut);
        delgates[(int)Method.BackInOut] = new Easing.EaseingDelgate(Back.EaseInOut);
        
        delgates[(int)Method.BounceIn] = new Easing.EaseingDelgate(Bounce.EaseIn);
        delgates[(int)Method.BounceOut] = new Easing.EaseingDelgate(Bounce.EaseOut);
        delgates[(int)Method.BounceInOut] = new Easing.EaseingDelgate(Bounce.EaseInOut);
        
        delgates[(int)Method.CircIn] = new Easing.EaseingDelgate(Circ.EaseIn);
        delgates[(int)Method.CircOut] = new Easing.EaseingDelgate(Circ.EaseOut);
        delgates[(int)Method.CircInOut] = new Easing.EaseingDelgate(Circ.EaseInOut);
        
        delgates[(int)Method.CubicIn] = new Easing.EaseingDelgate(Cubic.EaseIn);
        delgates[(int)Method.CubicOut] = new Easing.EaseingDelgate(Cubic.EaseOut);
        delgates[(int)Method.CubicInOut] = new Easing.EaseingDelgate(Cubic.EaseInOut);
        
        delgates[(int)Method.ElasticIn] = new Easing.EaseingDelgate(Elastic.EaseIn);
        delgates[(int)Method.ElasticOut] = new Easing.EaseingDelgate(Elastic.EaseOut);
        delgates[(int)Method.ElasticInOut] = new Easing.EaseingDelgate(Elastic.EaseInOut);
        
        delgates[(int)Method.ExpoIn] = new Easing.EaseingDelgate(Expo.EaseIn);
        delgates[(int)Method.ExpoOut] = new Easing.EaseingDelgate(Expo.EaseOut);
        delgates[(int)Method.ExpoInOut] = new Easing.EaseingDelgate(Expo.EaseInOut);
        
        delgates[(int)Method.QuadIn] = new Easing.EaseingDelgate(Quad.EaseIn);
        delgates[(int)Method.QuadOut] = new Easing.EaseingDelgate(Quad.EaseOut);
        delgates[(int)Method.QuadInOut] = new Easing.EaseingDelgate(Quad.EaseInOut);
        
        delgates[(int)Method.QuartIn] = new Easing.EaseingDelgate(Quart.EaseIn);
        delgates[(int)Method.QuartOut] = new Easing.EaseingDelgate(Quart.EaseOut);
        delgates[(int)Method.QuartInOut] = new Easing.EaseingDelgate(Quart.EaseInOut);
        
        delgates[(int)Method.QuintIn] = new Easing.EaseingDelgate(Quint.EaseIn);
        delgates[(int)Method.QuintOut] = new Easing.EaseingDelgate(Quint.EaseOut);
        delgates[(int)Method.QuintInOut] = new Easing.EaseingDelgate(Quint.EaseInOut);
        
        delgates[(int)Method.SineIn] = new Easing.EaseingDelgate(Sine.EaseIn);
        delgates[(int)Method.SineOut] = new Easing.EaseingDelgate(Sine.EaseOut);
        delgates[(int)Method.SineInOut] = new Easing.EaseingDelgate(Sine.EaseInOut);
        
    }
    
    #endregion
    
    #region Wrapping methods
    
    /// <summary>
    /// Wrap the specified _t around _length and by the given _mode.
    /// </summary>
    /// <param name='_t'>
    /// _t.
    /// </param>
    /// <param name='_lowerBound'>
    /// _lower bound.
    /// </param>
    /// <param name='_upperBound'>
    /// _upper bound.
    /// </param>
    /// <param name='_mode'>
    /// _mode.
    /// </param>
    static public float Wrap(float _t, float _length, WrapMode _mode)
    {
        //convert to 0-1
        float t = _t / _length;
        return Wrap(t, _mode) * _length;
    }
    
    /// <summary>
    /// Wrap the specified _t between 0-1 by the given _mode.
    /// </summary>
    /// <param name='_t'>
    /// _t.
    /// </param>
    /// <param name='_mode'>
    /// _mode.
    /// </param>
    static public float Wrap(float _t, WrapMode _mode)
    {   
        switch(_mode)
        {
            case WrapMode.Loop:
                _t = Mathf.Repeat(_t, 1);
                break;
            case WrapMode.PingPong:
                _t = Mathf.PingPong(_t, 1);
                break;
            case WrapMode.Once:
                _t = Mathf.Clamp01(_t);
                _t = _t == 1 ? 0 : _t;
                break;
            case WrapMode.Default:
            case WrapMode.ClampForever:
                _t = Mathf.Clamp01(_t);
                break;
        }
        return _t;
    }
    
    #endregion
    
    #region Easing Generics
    
    // ----------------------------------------------------------------------------
    // Get a cached easing delegate
    // ----------------------------------------------------------------------------
    public static EaseingDelgate GetEasingFunction ( Easing.Method method )
    {
        return delgates[(int)method];
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation float.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static float Ease ( float t, float b, float c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        return func ( t, b, c, d );
    }
    // ----------------------------------------------------------------------------
    // Easing equation float.
    //
    // param t  Current tval to ease
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static float Ease01(float t, Easing.Method method)
    {
        EaseingDelgate func = delgates[(int)method];
        return func(t, 0, 1, 1);
    }

    // ----------------------------------------------------------------------------
    // Easing equation Vector2.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector2 Ease ( float t, Vector2 b, Vector2 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        
        return new Vector2 ( x, y );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Vector2.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector2 Ease ( float t, ref Vector2 b, ref Vector2 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        
        return new Vector2 ( x, y );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Vector3.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector3 Ease ( float t, Vector3 b, Vector3 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        float z = func ( t, b.z, c.z, d );
        
        return new Vector3 ( x, y, z );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Vector3.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector3 Ease ( float t, ref Vector3 b, ref Vector3 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        float z = func ( t, b.z, c.z, d );
        
        return new Vector3 ( x, y, z );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Vector4.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector4 Ease ( float t, Vector4 b, Vector4 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        float z = func ( t, b.z, c.z, d );
        float w = func ( t, b.w, c.w, d );
        
        return new Vector4 ( x, y, z, w );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Vector4.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Vector4 Ease ( float t, ref Vector4 b, ref Vector4 c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float x = func ( t, b.x, c.x, d );
        float y = func ( t, b.y, c.y, d );
        float z = func ( t, b.z, c.z, d );
        float w = func ( t, b.w, c.w, d );
        
        return new Vector4 ( x, y, z, w );
    }
    
    // ----------------------------------------------------------------------------
    // Easing equation Color.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return   The correct value.
    // ----------------------------------------------------------------------------
    public static Color Ease ( float t, ref Color b, ref Color c, float d, Easing.Method method )
    {
        EaseingDelgate func = delgates[(int)method];
        
        float cr = func ( t, b.r, c.r, d );
        float cg = func ( t, b.g, c.g, d );
        float cb = func ( t, b.b, c.b, d );
        float ca = func ( t, b.a, c.a, d );
        
        return new Color ( cr, cg, cb, ca );
    }
    
    #endregion
    
    #region Easing Functions
    
    // ----------------------------------------------------------------------------
    // Note : The original equations are Robert Penner's work.
    // ----------------------------------------------------------------------------
    
    #region Easing None
    
    // ----------------------------------------------------------------------------
    // No easing, just returns target value
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return    The correct value.
    // ----------------------------------------------------------------------------
    public static float None (float t, float b, float c, float d)
    {
        return c;
    }
    
    #endregion
    
    #region Easing Linear
    
    // ----------------------------------------------------------------------------
    // Easing equation float for a simple linear tweening, with no easing.
    //
    // param t  Current time (in frames or seconds).
    // param b  Starting value.
    // param c  Target value.
    // param d  Expected easing duration (in frames or seconds).
    // return    The correct value.
    // ----------------------------------------------------------------------------
    public static float Linear (float t, float b, float c, float d) 
    {
        return b + ( ( c - b ) * ( t / d ) );
    }
    
    #endregion
    
    #region Easing Cubic
    
    // ----------------------------------------------------------------------------
    //  Cubic Easing Equations
    // ----------------------------------------------------------------------------
    public static class Cubic
    {
        // ----------------------------------------------------------------------------
        // Easing equation float for a cubic (t^3) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (c-b)*(t/=d)*t*t + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a cubic (t^3) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return (c-b)*((t=t/d-1)*t*t + 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a cubic (t^3) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if ((t/=d/2) < 1) return (c-b)/2*t*t*t + b;
            
            return (c-b)/2*((t-=2)*t*t + 2) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Quad
    
    // ----------------------------------------------------------------------------
    // Quadratic Easing Functions
    // ----------------------------------------------------------------------------
    public static class Quad
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quadratic (t^2) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (c-b) * (t/=d) * t + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quadratic (t^2) easing out: decelerating to zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return -(c-b) *(t/=d)*(t-2) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quadratic (t^2) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut  (float t, float b, float c, float d) 
        {
            if ((t/=d/2) < 1) return (c-b)/2*t*t + b;
            
            return -(c-b)/2 * ((--t)*(t-2) - 1) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Quart
    
    // ----------------------------------------------------------------------------
    // Quartic Easing Functions
    // ----------------------------------------------------------------------------
    public static class Quart
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quartic (t^4) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (c-b)*(t/=d)*t*t*t + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quartic (t^4) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return -(c-b) * ((t=t/d-1)*t*t*t - 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quartic (t^4) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if ((t/=d/2) < 1) return (c-b)/2*t*t*t*t + b;
            return -(c-b)/2 * ((t-=2)*t*t*t - 2) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Quint
    
    // ----------------------------------------------------------------------------
    // Quintic Easing Functions
    // ----------------------------------------------------------------------------
    public static class Quint
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quintic (t^5) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (c-b)*(t/=d)*t*t*t*t + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quintic (t^5) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return (c-b)*((t=t/d-1)*t*t*t*t + 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a quintic (t^5) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if ((t/=d/2) < 1) return (c-b)/2*t*t*t*t*t + b;
            
            return (c-b)/2*((t-=2)*t*t*t*t + 2) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Sine
    
    // ----------------------------------------------------------------------------
    // Sinusoidal Easing Functions
    // ----------------------------------------------------------------------------
    public static class Sine
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a sinusoidal (sin(t)) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return -(c-b) * Mathf.Cos(t/d * (Mathf.PI/2)) + (c-b) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a sinusoidal (sin(t)) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return (c-b) * Mathf.Sin(t/d * (Mathf.PI/2)) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a sinusoidal (sin(t)) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            return -(c-b)/2 * (Mathf.Cos(Mathf.PI*t/d) - 1) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Exponential
    
    // ----------------------------------------------------------------------------
    // Exponential Easing Functions
    // ----------------------------------------------------------------------------
    public static class Expo
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an exponential (2^t) easing in: accelerating from zero velocity.
        //
        //param t  Current time (in frames or seconds).
        //param b  Starting value.
        //param c  Target value.
        //param d  Expected easing duration (in frames or seconds).
        //return    The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (t==0) ? b : (c-b) * Mathf.Pow(2, 10 * (t/d - 1)) + b - (c-b) * 0.001f;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an exponential (2^t) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            return (t==d) ? b+(c-b) : (c-b) * 1.001f * (-Mathf.Pow(2, -10 * t/d) + 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an exponential (2^t) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if (t==0) return b;
            if (t==d) return b+(c-b);
            if ((t/=d/2) < 1) return (c-b)/2 * Mathf.Pow(2, 10 * (t - 1)) + b - (c-b) * 0.0005f;
            return (c-b)/2 * 1.0005f * (-Mathf.Pow(2, -10 * --t) + 2) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Circular
    
    // ----------------------------------------------------------------------------
    // Circular Easing Functions
    // ----------------------------------------------------------------------------
    public static class Circ
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a circular (sqrt(1-t^2)) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d)
        {
            return -(c-b) * (Mathf.Sqrt(1 - (t/=d)*t) - 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a circular (sqrt(1-t^2)) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d)
        {
            return (c-b) * Mathf.Sqrt(1 - (t=t/d-1)*t) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a circular (sqrt(1-t^2)) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if ((t/=d/2) < 1) return -(c-b)/2 * (Mathf.Sqrt(1 - t*t) - 1) + b;
            
            return (c-b)/2 * (Mathf.Sqrt(1 - (t-=2)*t) + 1) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Elastic
    
    // ----------------------------------------------------------------------------
    // Elastic Easing Functions
    // ----------------------------------------------------------------------------
    public static class Elastic
    {
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an elastic (exponentially decaying sine wave) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param a  Amplitude.
        // param p  Period.
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            if (t==0) return b;
            if ((t/=d)==1) return b+(c-b);
            float p =  d *.3f;
            float s = 0;
            float a = 0;
            if (a == 0f || a < Mathf.Abs((c-b))) {
                a = (c-b);
                s = p/4;
            } else {
                s = p/(2*Mathf.PI) * Mathf.Asin ((c-b)/a);
            }
            return -(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an elastic (exponentially decaying sine wave) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param a  Amplitude.
        // param p  Period.
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d)
        {
            if (t==0) return b;
            if ((t/=d)==1) return b+(c-b);
            float p = d*.3f;
            float s = 0;
            float a = 0;
            if (a == 0f || a < Mathf.Abs((c-b))) {
                a = (c-b);
                s = p/4;
            } else {
                s = p/(2*Mathf.PI) * Mathf.Asin ((c-b)/a);
            }
            return (a*Mathf.Pow(2,-10*t) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p ) + (c-b) + b);
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for an elastic (exponentially decaying sine wave) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param a  Amplitude.
        // param p  Period.
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if (t==0) return b;
            if ((t/=d/2)==2) return b+(c-b);
            float p =  d*(.3f*1.5f);
            float s = 0;
            float a = 0;
            if (a == 0f || a < Mathf.Abs((c-b))) {
                a = (c-b);
                s = p/4;
            } else {
                s = p/(2*Mathf.PI) * Mathf.Asin ((c-b)/a);
            }
            if (t < 1) return -.5f*(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
            return a*Mathf.Pow(2,-10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )*.5f + (c-b) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Back
    
    // ----------------------------------------------------------------------------
    // Back Easing Functions
    // ----------------------------------------------------------------------------
    public static class Back
    {
        // ----------------------------------------------------------------------------
        // Easing equation float for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param s  Overshoot ammount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            float s = 1.70158f;
            return (c-b)*(t/=d)*t*((s+1)*t - s) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param s  Overshoot ammount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            float s = 1.70158f;
            return (c-b)*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // param s  Overshoot ammount: higher s means greater overshoot (0 produces cubic easing with no overshoot, and the default value of 1.70158 produces an overshoot of 10 percent).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            float s =  1.70158f;
            if ((t/=d/2) < 1) return (c-b)/2*(t*t*(((s*=(1.525f))+1)*t - s)) + b;
            return (c-b)/2*((t-=2)*t*(((s*=(1.525f))+1)*t + s) + 2) + b;
        }
        
    }
    
    #endregion
    
    #region Easing Bounce
    
    // ----------------------------------------------------------------------------
    // Bounce Easing Functions
    // ----------------------------------------------------------------------------
    public static class Bounce
    {
        // ----------------------------------------------------------------------------
        // Easing equation float for a bounce (exponentially decaying parabolic bounce) easing in: accelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseIn (float t, float b, float c, float d) 
        {
            return (c-b) - EaseOut (d-t, 0, (c-b), d) + b;
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a bounce (exponentially decaying parabolic bounce) easing out: decelerating from zero velocity.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseOut (float t, float b, float c, float d) 
        {
            if ((t/=d) < (1/2.75f)) 
            {
                return (c-b)*(7.5625f*t*t) + b;
            }
            else if (t < (2/2.75f))
            {
                return (c-b)*(7.5625f*(t-=(1.5f/2.75f))*t + .75f) + b;
            }
            else if (t < (2.5f/2.75f))
            {
                return (c-b)*(7.5625f*(t-=(2.25f/2.75f))*t + .9375f) + b;
            }
            else
            {
                return (c-b)*(7.5625f*(t-=(2.625f/2.75f))*t + .984375f) + b;
            }
        }
        
        // ----------------------------------------------------------------------------
        // Easing equation float for a bounce (exponentially decaying parabolic bounce) easing in/out: acceleration until halfway, then deceleration.
        //
        // param t  Current time (in frames or seconds).
        // param b  Starting value.
        // param c  Target value.
        // param d  Expected easing duration (in frames or seconds).
        // return   The correct value.
        // ----------------------------------------------------------------------------
        public static float EaseInOut (float t, float b, float c, float d) 
        {
            if (t < d/2) return EaseIn (t*2, 0, (c-b), d) * .5f + b;
            
            else return EaseOut (t*2-d, 0, (c-b), d) * .5f + c*.5f + b;
        }
        
    }
    
    #endregion
    
    // ----------------------------------------------------------------------------
    
    #endregion
}