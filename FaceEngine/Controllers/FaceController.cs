using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FaceEngine.Controllers
{
    public class FaceController : ApiController
    {
        public int Dodaj(int a, int b)
        {
            return a + b;
        }
    }
}
