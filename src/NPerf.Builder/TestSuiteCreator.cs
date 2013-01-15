﻿namespace NPerf.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Fasterflect;
    using NPerf.Framework;
    using NPerf.Builder.Properties;
    using System.Linq.Expressions;

    public class TestSuiteCreator
    {
        private readonly Type testerType;

        private readonly Type testedAbstraction;

        private readonly MethodInfo runDescriptor;

        private readonly MethodInfo setUp;

        private readonly MethodInfo tearDown;

        private readonly MethodInfo[] methods;

        private readonly int defaultTestCount;

        private readonly string description;

        private readonly string featureDescription;

        public TestSuiteCreator(Type testerType, Type testedType)
        {
            if (testerType == null)
            {
                throw new ArgumentNullException("testerType");
            }

            if (testedType == null)
            {
                throw new ArgumentNullException("testedType");
            }

            this.testerType = testerType;
            var testerAttribute = this.testerType.Attribute<PerfTesterAttribute>();
            if (testerAttribute == null)
            {
                throw new ArgumentException("Tester type must be marked by PerfTesterAttribute", "testerType");
            }

            this.testedAbstraction = testerAttribute.TestedType;
            if (!this.testedAbstraction.IsAssignableFrom(testedType))
            {
                throw new ArgumentException(string.Format("Tester type {0} is not assignable from {1}", this.testedAbstraction, testedType), "testedType");
            }

            this.defaultTestCount = testerAttribute.TestCount;
            this.description = testerAttribute.Description;
            this.featureDescription = testerAttribute.FeatureDescription;

            // get run descriptor
            this.runDescriptor =
                this.testerType.MethodsWith(Flags.AllMembers, typeof(PerfRunDescriptorAttribute))
                    .FirstOrDefault(
                        m => m.ReturnType == typeof(double) && m.HasParameterSignature(new[] { typeof(int) }));

            // get set up
            this.setUp =
                this.testerType.MethodsWith(Flags.AllMembers, typeof(PerfSetUpAttribute))
                    .FirstOrDefault(
                        m => m.ReturnType == typeof(void) && m.HasParameterSignature(new[] { typeof(int), this.testedAbstraction }));

            // get tear down
            this.tearDown =
                this.testerType.MethodsWith(Flags.AllMembers, typeof(PerfTearDownAttribute))
                    .FirstOrDefault(
                        m => m.ReturnType == typeof(void) && m.HasParameterSignature(new[] { this.testedAbstraction }));

            // get test method
            this.methods =
                this.testerType.MethodsWith(Flags.AllMembers, typeof(PerfTestAttribute))
                    .Where(m => m.ReturnType == typeof(void) && m.HasParameterSignature(new[] { this.testedAbstraction }))
                    .ToArray();
        }

        private string CreateSourceCode()
        {
            var testSuiteCode = new TestSuiteCodeBuilder
            {
                SetUpMethodName = this.setUp == null ? string.Empty : this.setUp.Name,
                TearDownMethodName = this.tearDown == null ? string.Empty : this.tearDown.Name,
                DefaultTestCount = this.defaultTestCount,
                TestedAbstraction = this.testedAbstraction.Namespace + "." + this.testedAbstraction.Name,
                TypeToTest = this.testerType.Namespace + "." + this.testerType.Name,
                Description = this.description,
                FeatureDescription = this.featureDescription,
                Tests = (from method in
                             this.testerType.MethodsWith(Flags.AllMembers, typeof(PerfTestAttribute))
                         where method.ReturnType == typeof(void) && method.HasParameterSignature(new[] { this.testedAbstraction })
                         let testAttribute = method.Attribute<PerfTestAttribute>()
                         let ignoreAttribute = method.Attribute<PerfIgnoreAttribute>()
                         select new TestCodeBuider
                         {
                             Name = method.Name,
                             Description = testAttribute.Description,
                             IsIgnore = ignoreAttribute != null,
                             IgnoreMessage = ignoreAttribute == null ? string.Empty : ignoreAttribute.Message
                         }).ToArray()
            };

            return testSuiteCode.ToString();
        }
        /*
        public Assembly CreateTestSuite()
        { }*/
    }
}
