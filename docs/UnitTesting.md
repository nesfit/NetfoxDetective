# UnitTesting

All submitted code in a pull request needs to be tested using UnitTests or IntegrationTests! Use [extensions](https://github.com/nunit/nunit-vs-testgenerator) to automaticaly generate test classes and/or projects.
Tested projects need to include  ```[assembly: InternalsVisibleTo ("XXX.Tests")] ``` in its  ```Properties/Assembly.cs ```.
 
## New projects

When you create a new project, you are obligated to create a new UnitTest project as well. All UnitTest projects use [NUnit Framework](https://www.nunit.org/).
Add these NuGet packages to newly created test project.
* NUnit
* NUnit3TestAdapter
New UnitTest project has to be located in the same folder as the tested project. Alwayes change a configuration in ```Configuration Manager/Platform``` to ```x64``` and remove ```Any CPU```.

## Existing projects

When a test project exists, implement new tests, otherwise create new test project heading instructions for new projects.

## Naming conventions

Testing project has to be named after a project it tests, e.g., for **Framework** project the testing project name will be  **Framework.Tests**.
Test class has to be named after a class it tests, e.g., for **SnooperBase** class the testing class name will be **SnooperBaseTests** 
Test method names represents MethodName_StateUnderTest_ExpectedBehavior:
 * isAdult_AgeLessThan18_False
 * withdrawMoney_InvalidAccount_ExceptionThrown
 * admitStudent_MissingMandatoryFields_FailToAdmit

