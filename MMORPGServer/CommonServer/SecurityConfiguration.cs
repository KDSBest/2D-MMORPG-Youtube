﻿namespace CommonServer
{
    public static class SecurityConfiguration
    {
        // TODO Read from env variable for k8s
        public static readonly string JwtSecret = "HelloWorld!HelloWorld!HelloWorld!HelloWorld!";

        public static readonly string EmailClaimType = "email";
    }
}
