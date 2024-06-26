namespace Soil {

/// a transform over a float value
public interface FloatTransform {
    /// transform the value
    float Evaluate(float value);
}

}