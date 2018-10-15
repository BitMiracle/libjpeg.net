1. If you want to use LibJpeg.Net as external assembly in your project.

    Simply add reference to BitMiracle.LibJpeg.NET.dll and then use classes from namespaces <xref:BitMiracle.LibJpeg> or <xref:BitMiracle.LibJpeg.Classic>. Usually you also need to use System.IO namespace. 

2. If you embed some code files from LibJpeg.Net into your project.
    If you want to mark imported classes as "public" just build your project with compilation symbol **EXPOSE_LIBJPEG**.
    
    If you don't declare this symbol then all imported classes will be declared as "internal".

