// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Samples.DefaultParam
{
    internal class Program
    {
#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type

        private static void BasicExample()
        {
            // This is a new instance of the Logger<T> class written by the user.
            Logger<string> logger = new();

            // This is a new instance of the generated, Logger<T>-based Logger class.
            // Logger has the TypeConvention.Copy applied.
            Logger generatedLogger = new();

            // The usage of 'string' is identical to Logger<string>.
            logger.Info("Logger Test");
            generatedLogger.Info("Logger Test");

            // The generated class does not inherit Logger<string>...
            Console.WriteLine(generatedLogger is Logger<string>);

            // ...but it does implement the interfaces of Logger<string>.
            Console.WriteLine(generatedLogger is ILogger<string>);
        }

        private static void InheritanceExample()
        {
            // InheritedLogger<T> has the TypeConvention.Inherit applied.
            InheritedLogger<string> logger = new();
            InheritedLogger generatedLogger = new();

            // Like previously, experience is exactly the same.
            logger.Info("Logger Test");
            generatedLogger.Info("Logger Test");

            // The generated class does inherit InheritedLogger<string>...
            Console.WriteLine(generatedLogger is InheritedLogger<string>);

            // ...and all of its base classes, like Logger<string>...
            Console.WriteLine(generatedLogger is Logger<string>);

            // ...as well ass all interfaces...
            Console.WriteLine(generatedLogger is ILogger<string>);

            // ...but not the generated Logger.
            Console.WriteLine(generatedLogger is Logger);
        }

#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type

        private static void Main()
        {
            BasicExample();
            InheritanceExample();
        }
    }
}