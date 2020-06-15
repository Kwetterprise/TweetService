using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Kwetterprise.TweetService.Common.DataTransfer
{
    public class TimedData<T>
    {
        public TimedData()
        {
        }
        public TimedData(List<T> data, bool ascending, Guid? next)
        {
            this.Data = data;
            this.Ascending = ascending;
            this.Next = next;
        }

        public Guid? Next { get; set; }

        public bool Ascending { get; set; }

        [Required]
        public List<T> Data { get; set; }

        public TimedData<TOut> Select<TOut>(Func<T, TOut> selector)
        {
            return new TimedData<TOut>(this.Data.Select(selector).ToList(), this.Ascending, this.Next);
        }
    }
}