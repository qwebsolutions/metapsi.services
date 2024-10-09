#!/bin/sh

repos=($metapsi)

metapsi=c:/github/qwebsolutions/metapsi

projects=( \
$metapsi/Metapsi.Runtime \
$metapsi/Metapsi.Mds \
$metapsi/Metapsi.Redis \
$metapsi/Metapsi.Web \
$metapsi/Metapsi.Web.Contracts \
$metapsi/Metapsi.Module \
$metapsi/Metapsi.JavaScript \
$metapsi/Metapsi.Html \
$metapsi/Metapsi.Heroicons \
$metapsi/Metapsi.Shoelace \
$metapsi/Metapsi.SQLite \
$metapsi/Metapsi.TomSelect \
$metapsi/Metapsi.ChoicesJs \
$metapsi/Metapsi.Timer \
$metapsi/Metapsi.Messaging \
$services/Metapsi.ServiceDoc)

echo ${projects[*]}