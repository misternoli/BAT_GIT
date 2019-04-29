clear all; 
clc; 

filename = 'Matlab_0_01_to_0_5.xlsx';
sheet = 1;
xlRange = 'A:E';
[num,txt,raw] = xlsread(filename,sheet,xlRange);
t= num(1:end,1); u= num(1:end,4); T_Zelle=num(1:end,2); T_Mantel=num(1:end,3) ;T_Umgebung=num(1:end,5);
dT= T_Zelle(end:end)-T_Zelle(1:1);
fileende=size(t,1);
Zeitende=fileende/2;
%%
%In einem Plot
figure('Name','Zelle');
subplot(2,1,1);
plot(t,T_Mantel,'b',t,T_Zelle,'r');
title('Temperaturverlauf - Sprung von 0.005 auf 0.01')
legend('T_{Mantel}','T_{Zelle}','Location','best');
F_Beschriftung;

subplot(2,1,2);
plot(t,T_Umgebung);
title('Ambient Temperature ')
legend('T_{Umgebung}','Location','best');
F_Beschriftung;
%%
%Zwei Plots 
%Temperaturverlauf Zelle
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
%Nullpunkt als Startwert auslegen
figure(5)
T_Mantel0 = T_Mantel-T_Mantel(1:1);
T_Zelle0 = T_Zelle-T_Zelle(1:1);
plot(t,T_Mantel0,t,T_Zelle0)
axis([0 12000 -60  0])
%%
%-------------------------------
systemIdentification('Cooling_iden')
%%
%Mantelparameter einzeln betrachtet
Kp_Mantel=P2.KP;
T1_Mantel=P2.Tp1;
T2_Mantel=P2.Tp2;
G_Mantel= tf([Kp_Mantel],[T2_Mantel*T1_Mantel T2_Mantel+T1_Mantel 1])
%%
%Bioreaktorparameter einzeln betrachtet
Kp_Reaktor =  P3.Kp;
T1_Reaktor =  P3.Tp1;
T2_Reaktor =  P3.Tp2;
T3_Reaktor =  P3.Tp3;
G_Reaktor  =  tf([Kp_Reaktor],[T2_Reaktor*T1_Reaktor T1_Reaktor+T2_Reaktor 1])*tf([1],[T3_Reaktor 1])
%Zelle
Kp_Zelle = Kp_Reaktor/Kp_Mantel;
T1_Zelle = T3_Reaktor;
G_Zelle = tf([Kp_Zelle],[T1_Zelle 1])
%%
G_Z_Mantel=c2d(G_Mantel,0.5)
G_Z_Zelle=c2d(G_Zelle,0.5)
%%
sim('Cold_Mantel');
figure('Name','Zelle');
plot(T_M.time, T_M.signals.values,'b')
%%
%Differenz von Simulation und Real
T_dZelle=[23140:1];
T_dMantel=[23140:1];
for x=1:fileende
    T_dZelle(x,1)=T_Zelle0(x,1)-T_Z.signals.values(x,1);
end
for x=1:fileende
    T_dMantel(x,1)=T_Mantel0(x,1)-T_M.signals.values(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,T_dMantel,'b', t,T_dZelle, 'r')

%%
%Simulation Reaktortemperatorverlauf mit PT2 Glied 
G_Z_Reaktor=c2d(G_Reaktor,0.5) 
sim('ColdPT2');
figure('Name','Reaktor');
plot(T_Reaktor.time, T_Reaktor.signals.values)

%Differenz von Simulation und Real
T_Diff=[23140:1];
for x=1:fileende
    T_Diff(x,1)=T_Zelle0(x,1)-T_Reaktor.signals.values(x,1);
end
figure('Name','Differenz T_Real zu T_Simulation');
plot(t,T_Diff)


%%
function F_Beschriftung
y=ylabel('Temperatur [°C]');y.FontSize=12;
x=xlabel('Zeit [s]'); x.FontSize=12;
lgd=legend('boxoff')
lgd.FontSize = 12;
end