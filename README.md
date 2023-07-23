# Pantree.Server

[![ci](https://github.com/benhollar/Pantree.Server/workflows/CI/badge.svg)](https://github.com/benhollar/Pantree.Server/actions?query=workflow%3ACI+branch%3Amain)
[![codecov](https://codecov.io/gh/benhollar/Pantree.Server/branch/main/graph/badge.svg?token=GK9GS89FX5)](https://codecov.io/gh/benhollar/Pantree.Server)

**Development of this repository is still nascent and in flux.**

`Pantree.Server` is a .NET REST API designed to allow client applications of any type to create and manage recipes,
pantry inventory, and personal meal plans. It builds upon [`Pantree.Core`](https://github.com/benhollar/Pantree.Core)
for the fundamental data models, but database persistence and other features relevant only to the REST API are
implemented here.
