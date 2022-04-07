# How to contribute

If you choose to contribute in this repository, please follow these steps:

## Choose an issue

All contributions should be linked to an issue within this repository. If you find a feature or a bug that is not currently tracked
by any issue, create a new issue describing the bug or proposing a new feature.

Each issue should be appropriatly labeled with `type-enhancement` or `type-bug`.

## Workflow

The typical workflow for contributing to this repository is described bellow. However, it will not be forced, but it will help us to
ensure a quality of PR.

- Set up your environment for you to build and test the code and create a fork for your work.
- Always make sure, that all tests are passing.
- Choose an issue
- Create and check out a branch in your local clone. This branch will be used to prepare your PR.
- Always consider adding new tests for your changes, better to have to many tests then none.
- When you are done with changes, make sure all tests are still passing (`dotnet test` command).
- Commit changes to your branch a push it to your GitHub fork.
- In the main `Neo4j.OGM` repo you should have now a message in yellow box suggesting you create a PR from your fork. Do this, or create the PR by other means.
- Wait for feedback from the team and for the CI checks to pass.
- Add and push new commits to your branch to address any issues.

PR will be merged by a member of the team once the CI checks have passed and the code has been approved.
