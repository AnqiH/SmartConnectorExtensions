Most of the POCO classes in this namespace were code generated based on the XML values returned from experimentation with the CSP Proxy.  
I did take liberties with property naming and such to make it adhere to normal conventions.  

This will become a bit of a maintenance hurdle I fear as I did encounter some properties not returned (due to "nullness") in some cases.

Some of the classes here are abstracted encapsulation in order to better handle some "record" retrieval (e.g. CspTrendSample and CspAlarmEvent).
I did this simply because CSP is very abstract and I don't like that at all.

Interfaces and base classes were added as needed to better handle some polymorphic tasks.