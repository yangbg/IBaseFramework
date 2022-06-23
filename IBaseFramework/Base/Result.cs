using System;
using System.Threading.Tasks;

namespace IBaseFramework.Base
{
    /// <summary> 基础数据结果类 </summary>
    [Serializable]
    public class Result<T> : Result
    {
        public T Data { get; set; }
    }


    /// <summary>
    /// 返回结果集
    /// </summary>
    [Serializable]
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }

        public static Result Success(string message = "")
        {
            return new Result()
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static Task<Result> SuccessAsync(string message = "")
        {
            return Task.FromResult(Success(message));
        }

        public static Result<T> Success<T>(T data, string message = "")
        {
            return new Result<T>()
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static Task<Result<T>> SuccessAsync<T>(T data, string message = "")
        {
            return Task.FromResult(Success(data, message));
        }

        public static Result Error(string message)
        {
            return new Result()
            {
                IsSuccess = false,
                Message = message
            };
        }

        public static Task<Result> ErrorAsync(string message)
        {
            return Task.FromResult(Error(message));
        }

        public static Result<T> Error<T>(string message)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }

        public static Task<Result<T>> ErrorAsync<T>(string message)
        {
            return Task.FromResult(Error<T>(message));
        }

        public static Result<T> Error<T>(T data, string message)
        {
            return new Result<T>()
            {
                IsSuccess = false,
                Message = message,
                Data = data
            };
        }

        public static Task<Result<T>> ErrorAsync<T>(T data, string message)
        {
            return Task.FromResult(Error(data, message));
        }
    }
}