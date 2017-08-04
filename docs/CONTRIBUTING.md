# Contributing

Current git branching model is **GitFlow**. In case you are not familiar with
this branching model, several introductory articles are available
online[^1] [^2].

Users of *Microsoft Visual Studio* can use [GitFlow for Visual Studio 2017](https://marketplace.visualstudio.com/items?itemName=vs-publisher-57624.GitFlowforVisualStudio2017).

## Workflow notes
> "We consider `origin/master` to be the main branch where the source code
>  of `HEAD` always reflects a production-ready state."[^1]

> "We consider `origin/develop` to be the main branch where the source code
> of `HEAD` always reflects a state with the latest delivered development
> changes for the next release."[^1]

> "The essence of a *feature branch* is that it exists as long as the feature is
> in development, but will eventually be merged back into develop
> (to definitely add the new feature to the upcoming release) or discarded
> (in case of a disappointing experiment)."[^1]

New features are developed in individual *feature branches* that start from
the *develop branch*.

1. When starting work on a new feature, please create individual *feature
  branch* from the current *develop branch*. It is recommended to push
  your *feature branch* to *origin*.
2. Develop feature, documentation, tests, ...
3. The feature in *feature branch* is completed.
4. Update your *feature branch* by merging current *develop branch* to your
  *feature branch*. Solve possible merge conflicts.
5. Push successfully updated *feature branch* to *origin*.
6. Submit pull request for merging your *feature branch* to *develop branch*,
  therefore `origin/feature/my-work` to `origin/develop`.

Based on the above described *GitFlow*, it is advised **not to commit any
changes to the *master branch*** during daily development.

![feature branch diagram](/docs/images/feature_branch_diagram.png)

---

[^1]: A successful Git branching model URL: http://nvie.com/posts/a-successful-git-branching-model/

[^2]: git-flow cheatsheet URL: https://danielkummer.github.io/git-flow-cheatsheet/
