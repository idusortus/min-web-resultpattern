using SharedKernel;

namespace Api.Extensions;

public static class ResultExtensions
{
    /// <summary>
    /// Transforms a non-generic Result into a single output value by executing one of two provided functions.
    /// This allows you to handle both success and failure states in a single, fluent expression without an if/else block.
    /// </summary>
    /// <typeparam name="TOut">The type of the output value to be returned.</typeparam>
    /// <param name="result">The Result object to be matched.</param>
    /// <param name="onSuccess">The function to execute if the result is successful. It takes no input and must return a TOut value.</param>
    /// <param name="onFailure">The function to execute if the result is a failure. It receives the failed Result object and must return a TOut value.</param>
    /// <returns>The output value of type TOut from whichever function was executed.</returns>
    public static TOut Match<TOut>(this Result result,
    Func<TOut> onSuccess,
    Func<Result, TOut> onFailure)
    {
        // Check the IsSuccess flag on the Result object.
        // If it's true, invoke the onSuccess function.
        // If it's false, invoke the onFailure function, passing in the entire result object
        // so the caller can inspect the error details.
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result);
    }

    /// <summary>
    /// Transforms a generic Result<TIn> into a single output value by executing one of two provided functions.
    /// This allows you to handle both success (with its value) and failure states in a single, fluent expression.
    /// </summary>
    /// <typeparam name="TIn">The type of the Value contained within a successful Result.</typeparam>
    /// <typeparam name="TOut">The type of the output value to be returned.</typeparam>
    /// <param name="result">The Result<TIn> object to be matched.</param>
    /// <param name="onSuccess">The function to execute if the result is successful. It receives the result's Value (of type TIn) and must return a TOut value.</param>
    /// <param name="onFailure">The function to execute if the result is a failure. It receives the failed Result object and must return a TOut value.</param>
    /// <returns>The output value of type TOut from whichever function was executed.</returns>
    public static TOut Match<TIn, TOut>(this Result<TIn> result,
    Func<TIn, TOut> onSuccess,
    Func<Result<TIn>, TOut> onFailure)
    {
        // Check the IsSuccess flag on the Result<TIn> object.
        // If it's true, invoke the onSuccess function, passing in the successful Value from the result.
        // If it's false, invoke the onFailure function, passing in the entire result object.
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result);
    }
}