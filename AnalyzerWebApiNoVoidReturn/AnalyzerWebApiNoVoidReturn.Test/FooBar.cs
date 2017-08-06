using System;
using System.Web.Http;

namespace Test
{
    public class FooBar : ApiController
    {
        public int ViolatingMethod(int a)
        {
            if (a > 0)
            {
                return new Random().Next();
            }
            else
            {
                return new Random().Next();
            }
        }
    }
}