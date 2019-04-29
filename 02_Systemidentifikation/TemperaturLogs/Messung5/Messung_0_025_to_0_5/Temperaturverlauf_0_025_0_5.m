clear all; 
clc; 

filename = 'Messung_0_025_to_0_5\Matlab_0_025_to_0_5.xlsx';
sheet = 1;
xlRange = 'A:E';
[num,txt,raw] = xlsread(filename,sheet,xlRange);
%t= num(1:fileende,1); u= num(1:fileende,5); T_Zelle=num(1:fileende,2); T_Mantel=num(1:fileende,3); T_Umgebung=num(1:fileende,4);
t= num(1:end,1); u= num(1:end,5); T_Zelle=num(1:end,2); T_Mantel=num(1:end,3); T_Umgebung=num(1:end,4);
dT= T_Zelle(end:end)-T_Zelle(1:1);
fileende=size(t,1);
Zeitende=fileende/2;
%%
%In einem Plot
close all
figure('Name','Zelle');
subplot(2,1,1);
plot(t,T_Mantel,'b',t,T_Zelle,'r'); 
%title('Temperaturverlauf - Sprung von 0.005 auf 0.01');
legend('T_{Mantel}','T_{Zelle}','Location','southeast');
F_Beschriftung;
subplot(2,1,2);
plot(t,T_Umgebung);
title('Umgebungstemperatur')
F_Beschriftung;
legend('T_{Umgebung}','Location','best');

%%
%0Punkt für SystemIdentification
T_min= min(T_Zelle);
T_Zelle0 = T_Zelle-T_min;
figure('Name','Nullpunkt')
plot(t,T_Zelle0); 
axis([0 Zeitende 0  dT+1])       %axis([xmin xmax ymin ymax])
title('Temperaturverlauf - Sprung von 0.005 auf 0.01')
F_Beschriftung;
lgd=legend('T_{Zelle}','Location','southeast');
%%
systemIdentification();

%%
Kp_Mantel=P1.KP;
T1_Mantel=P1.Tp1;
G_Mantel= tf([Kp_Mantel],[T1_Mantel 1])
%Bioreaktorparameter einzeln betrachtet
Kp_Reaktor =  P3D.Kp;
T1_Reaktor =  P3D.Tp1;
T2_Reaktor =  P3D.Tp2;
T3_Reaktor =  P3D.Tp3;
Td_Z       =  P3D.Td;           %Totzeit
G_Reaktor  =  tf([Kp_Reaktor],[T2_Reaktor*T1_Reaktor T1_Reaktor+T2_Reaktor 1])*tf([1],[T3_Reaktor 1])
%%
%Simulation nacheinander
Kp_Zelle=Kp_Reaktor/Kp_Mantel;
G_Zelle= tf([Kp_Zelle],[T3_Reaktor*T2_Reaktor T2_Reaktor+T3_Reaktor 1]);
% Diskretisieren
G_Z_Mantel=c2d(G_Mantel,0.5);
G_Z_Zelle=c2d(G_Zelle,0.5);
%%
% Simulation
sim('Heating_sequential');
figure('Name','Heating_Seperated');
plot(T_M.time, T_M.signals.values,'b', T_Z.time, T_Z.signals.values, 'r')
legend('T_{Mantel}','T_{Zelle}','Location','best')
F_Beschriftung;
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
legend('T_{Mantel}','T_{Zelle}','Location','best')
F_Beschriftung;

%%
function F_Beschriftung
y=ylabel('Temperatur [°C]');y.FontSize=12;
x=xlabel('Zeit [s]'); x.FontSize=12;
lgd=legend('boxoff');
lgd.FontSize = 12;
end