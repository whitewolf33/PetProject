# PetProject



<h3>Introduction</h3>

Pet Project is a console application built on .Net Core (2.2) that could run on any platform such as Windows, Linux and MacOS.
The main intent of this application is to consume a web API that returns a list of pets registered under their respective owners.

<h3>Contributions:</h3>

    Vinod Srinivasan

<h3>Version</h3>

    Version 1.0

<h3>Features</h3>
<ul>
    <li>Resilient - The  application is resilient with the ability to retry three times (using Polly libraries) </li>
    <li>Logging - The app has logging configured and could be extended to log to a file if required</li>
    <li>Extendable - The core logic is in a BusinessLogic dll and this could be used with other clients if required</li>
</ul>

<h3>Tests</h3>

    Unit Tests using MSTest and Moq
    Integration Tests using MSTest talking to the live API.

<h3>Support</h3>
    Any questions or concerns, please create an issue with details.
