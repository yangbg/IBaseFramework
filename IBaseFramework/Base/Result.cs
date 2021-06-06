using System;
using System.Threading.Tasks;

namespace IBaseFramework.Base
{
    /// <summary>
    /// 返回结果集
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success()
        {
            return new Result(true, string.Empty);
        }

        public static Task<Result> SuccessAsync()
        {
            return Task.FromResult(new Result(true, string.Empty));
        }

        public static Result Success(string message)
        {
            return new Result(true, message);
        }

        public static Task<Result> SuccessAsync(string message)
        {
            return Task.FromResult(new Result(true, message));
        }

        public static Result<T> Success<T>(T data)
        {
            return new Result<T>(data);
        }

        public static Task<Result<T>> SuccessAsync<T>(T data)
        {
            return Task.FromResult(new Result<T>(data));
        }

        public static Result Error()
        {
            return new Result(false, string.Empty);
        }

        public static Task<Result> ErrorAsync()
        {
            return Task.FromResult(new Result(false, string.Empty));
        }

        public static Result Error(string message)
        {
            return new Result(false, message);
        }

        public static Task<Result> ErrorAsync(string message)
        {
            return Task.FromResult(new Result(false, message));
        }

        public static Result<T> Error<T>(T data, string message)
        {
            return new Result<T>(data, message);
        }

        public static Task<Result<T>> ErrorAsync<T>(T data, string message)
        {
            return Task.FromResult(new Result<T>(data, message));
        }

        public static Result<T> Error<T>(string message)
        {
            return new Result<T>(message);
        }

        public static Task<Result<T>> ErrorAsync<T>(string message)
        {
            return Task.FromResult(new Result<T>(message));
        }
    }

    /// <summary> 基础数据结果类 </summary>
    [Serializable]
    public class Result<T> : Result
    {
        public T Data { get; set; }

        public Result(T data) : base(true, string.Empty)
        {
            Data = data;
        }

        public Result(string message) : base(false, message)
        {

        }

        public Result(T data, string message) : base(false, message)
        {
            Data = data;
        }
    }
}