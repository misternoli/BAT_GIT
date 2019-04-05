clear all; 
clc; 

filename = 'C:\Users\arbno\Dropbox\BAT\02_Systemidentifikation\TemperaturLogs\Messung2\TempLogs\Heating\Matlab.xlsx';
sheet = 1;
fileende=100000;
Zeitende=fileende/2;
xlRange = 'A:E';
[num,txt,raw] = xlsread(filename,sheet,xlRange);
t= num(1:100000,1); u= num(1:100000,4); T_Zelle=num(1:100000,2); T_Mantel=num(1:100000,3); T_Umgebung=num(1:fileende,5);
%%
%In einem Plot
close all
figure('Name','Zelle');
subplot(2,1,1);
plot(t,T_Mantel,'b',t,T_Zelle,'r'); 
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
title('Temperaturverlauf - Sprung von 0.005 auf 0.01')
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')

subplot(2,1,2);
plot(t,T_Umgebung);
title('Ambient Temperature ')
legend('T_{Umgebung}','Location','southeast') ;legend('boxoff')
%%
%Zwei Plots 
%Temperaturverlauf Zelle
close all
figure('Name','Temperaturverlauf Zelle');
plot(t,T_Zelle,'r'); ylabel('T_Zelle');
x0=0;
ymax=T_Zelle(100000:100000);
line([x0 50000], [ymax ymax])
ymin=T_Zelle(1:1);
line([x0 50000], [ymin ymin])
%Temperaturverlauf Mantel
figure('Name','Temperaturverlauf Mantel');
plot(t,T_Mantel,'r');% ylabel('T_Mantel')
ymax=T_Mantel(100000:100000);
line([x0 50000], [ymax ymax])
ymin=T_Mantel(1:1);
line([x0 50000], [ymin ymin])

%%
%0Punkt für SystemIdentification
T_Mantel0 = T_Mantel-T_Mantel(1:1);
T_Zelle0 = T_Zelle-T_Zelle(1:1);
figure('Name','Nullpunkt')
plot(t,T_Mantel0,t,T_Zelle0); 
axis([0 Zeitende 0  12])       %axis([xmin xmax ymin ymax])
title('Temperaturverlauf - Sprung von 0.005 auf 0.01')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')
%%
systemIdentification('..\Heating_sysid.sid')
%%
Kp_Mantel=P1.KP;
T1_Mantel=P1.Tp1;
G_Mantel= tf([Kp_Mantel],[T1_Mantel 1])
%Bioreaktorparameter einzeln betrachtet
Kp_Reaktor =  P3.Kp;
T1_Reaktor =  P3.Tp1;
T2_Reaktor =  P3.Tp2;
T3_Reaktor =  P3.Tp3;

G_Reaktor  =  tf([Kp_Reaktor],[T2_Reaktor*T1_Reaktor T1_Reaktor+T2_Reaktor 1])*tf([1],[T3_Reaktor 1])
%%
% Diskretisieren Seperat
G_Z_Mantel=c2d(G_Mantel,0.5)
G_Z_Zelle=c2d(G_Reaktor,0.5)
%---------------------------------------
%%
sim('Heating_Seperated');
figure('Name','Heating_Seperated');
plot(T_M.time, T_M.signals.values,'b', T_Z.time, T_Z.signals.values, 'r','DisplayName', 'T_Z')
title('Simulation - PT1 & PT3')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Mantel}','T_{Reaktor}','Location','southeast') ;legend('boxoff')
%%
%Differenz von Simulation und Real
T_dZelle=[fileende:1];
T_dMantel=[fileende:1];
for x=1:fileende
    T_dZelle(x,1)=T_Zelle0(x,1)-T_Z.signals.values(x,1);
end
for x=1:fileende
    T_dMantel(x,1)=T_Mantel0(x,1)-T_M.signals.values(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,T_dMantel,'b', t,T_dZelle, 'r')
title('Differenz Simulation - Real')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Mantel}','T_{Reaktor}','Location','southeast') ;legend('boxoff')
%%
%Simulation nacheinander
Kp_Zelle=Kp_Reaktor/Kp_Mantel;
G_Zelle= tf([Kp_Zelle],[T3_Reaktor*T2_Reaktor T2_Reaktor+T3_Reaktor 1]);
% Diskretisieren
G_Z_Mantel=c2d(G_Mantel,0.5)
G_Z_Zelle=c2d(G_Zelle,0.5);
%%
% Simulation
sim('Heating_sequential');
figure('Name','Heating_Seperated');
plot(T_M.time, T_M.signals.values,'b', T_Z.time, T_Z.signals.values, 'r')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')