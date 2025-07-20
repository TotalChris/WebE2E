# Web E2E
*A base Web E2E project written in .NET 8, NUnit, and Playwright*

Uses some custom test generation and attribute to permutate different browsers and environments.

In the default implementation, three example environment URLs and three browser engines are defined, creating nine permutations per-test. Playwright could be easily swapped out for a different automation tool like Selenium but I chose Playwright for this template because of its simplicity and flaking protection.

Uniquely, and because of NUnit constraints related to how `[SetUp]` and `[OneTimeSetUp]` attributes work, the test generator wraps the provided test method in a custom `RunTest` method that provides initialization and prevents the need for the test to accept params (although you can pass params in the `Invoke()` call).
