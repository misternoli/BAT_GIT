clear all; 
clc; 

filename = 'Matlab_0_005-0_007.xlsx';
sheet = 1;
%Ohne Raumtemperaturerhöhung-----------
fileende=168000;
%-------------------------------------
%Mit Raumtemperaturerhöhung-----------
%fileende=198000;
%-------------------------------------
Zeitende=fileende/2;
xlRange = 'A:E';
[num,txt,raw] = xlsread(filename,sheet,xlRange);
t= num(1:fileende,1); u= num(1:fileende,4)-0.005; T_Zelle=num(1:fileende,2); T_Mantel=num(1:fileende,3); T_Umgebung=num(1:fileende,5);
%%
%In einem Plot
close all
figure('Name','Zelle');
subplot(2,1,1);
plot(t,T_Zelle,'r',t,T_Mantel,'b'); ylabel('Temperatur [°C]');
title('Step response')
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')
subplot(2,1,2);
plot(t,T_Umgebung);
title('Ambient Temperature ')
legend('T_{Umgebung}','Location','southeast') ;legend('boxoff')
%%
%Differenz von Mantel Zelle
dT=[fileende:1];
for x=1:fileende
    dT(x,1)=T_Mantel(x,1)-T_Zelle(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,dT,'b')
title('Differenz Simulation - Real')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Delta}','Location','southeast') ;legend('boxoff')

%%
%Zwei Plots 
%Temperaturverlauf Zelle
close all
figure('Name','Temperaturverlauf Zelle');
plot(t,T_Zelle,'r'); ylabel('T_Zelle');
x0=0;
ymax=T_Zelle(fileende:fileende);
line([x0 Zeitende], [ymax ymax])
ymin=T_Zelle(1:1);
line([x0 Zeitende], [ymin ymin])
%Temperaturverlauf Mantel
figure('Name','Temperaturverlauf Mantel');
plot(t,T_Mantel,'r');% ylabel('T_Mantel')
ymax=T_Mantel(fileende:fileende);
line([x0 Zeitende], [ymax ymax])
ymin=T_Mantel(1:1);
line([x0 Zeitende], [ymin ymin])

%%
%Startpunkt bei 0°
figure(5)
T_Mantel0 = T_Mantel-T_Mantel(1:1);
T_Zelle0 = T_Zelle-T_Zelle(1:1);
subplot(2,1,1);
plot(t,T_Mantel0,t,T_Zelle0)
title('Step response')
legend('T_{Mantel}','T_{Zelle}','Location','southeast') ;legend('boxoff')
subplot(2,1,2);
plot(t,T_Umgebung);
title('Ambient Temperature ')
legend('T_{Umgebung}','Location','southeast') ;legend('boxoff')
%%
%Parametersierung
systemIdentification('SysId_0_005_to_0_007')
%P1 --Mantel
%P3 --Zelle
%%
Kp_Mantel=P1.KP;
T1_Mantel=P1.Tp1;
G_Mantel= tf([Kp_Mantel],[T1_Mantel 1])
%Bioreaktorparameter einzeln betrachtet
Kp_Reaktor =  P3.Kp;
T1_Reaktor =  P3.Tp1;
T2_Reaktor =  P3.Tp2;
T3_Reaktor =  P3.Tp3;

G_Reaktor  =  tf([Kp_Reaktor],[T3_Reaktor T2_Reaktor T1_Reaktor 1])

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
